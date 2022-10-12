using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class HubDispatcher : PersistentConnection
	{
		private class ClientHubInfo
		{
			public string Name
			{
				get;
				set;
			}
		}

		private const string HubsSuffix = "/hubs";

		private const string JsSuffix = "/js";

		private readonly List<HubDescriptor> _hubs = new List<HubDescriptor>();

		private readonly bool _enableJavaScriptProxies;

		private readonly bool _enableDetailedErrors;

		private IJavaScriptProxyGenerator _proxyGenerator;

		private IHubManager _manager;

		private IHubRequestParser _requestParser;

		private JsonSerializer _serializer;

		private IParameterResolver _binder;

		private IHubPipelineInvoker _pipelineInvoker;

		private IPerformanceCounterManager _counters;

		private bool _isDebuggingEnabled;

		private static readonly MethodInfo _continueWithMethod = TypeExtensions.GetMethod(typeof(HubDispatcher), "ContinueWith", BindingFlags.Static | BindingFlags.NonPublic);

		protected override ILogger Logger => base.LoggerFactory.CreateLogger<HubDispatcher>();

		internal override string GroupPrefix => "hg-";

		public HubDispatcher(IOptions<SignalROptions> optionsAccessor)
		{
			if (optionsAccessor == null)
			{
				throw new ArgumentNullException("optionsAccessor");
			}
			SignalROptions value = optionsAccessor.Value;
			_enableJavaScriptProxies = value.Hubs.EnableJavaScriptProxies;
			_enableDetailedErrors = value.Hubs.EnableDetailedErrors;
		}

		public override void Initialize(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			IJavaScriptProxyGenerator proxyGenerator;
			if (!_enableJavaScriptProxies)
			{
				IJavaScriptProxyGenerator javaScriptProxyGenerator = new EmptyJavaScriptProxyGenerator();
				proxyGenerator = javaScriptProxyGenerator;
			}
			else
			{
				proxyGenerator = serviceProvider.GetRequiredService<IJavaScriptProxyGenerator>();
			}
			_proxyGenerator = proxyGenerator;
			_manager = serviceProvider.GetRequiredService<IHubManager>();
			_binder = serviceProvider.GetRequiredService<IParameterResolver>();
			_requestParser = serviceProvider.GetRequiredService<IHubRequestParser>();
			_serializer = serviceProvider.GetRequiredService<JsonSerializer>();
			_pipelineInvoker = serviceProvider.GetRequiredService<IHubPipelineInvoker>();
			_counters = serviceProvider.GetRequiredService<IPerformanceCounterManager>();
			base.Initialize(serviceProvider);
		}

		protected override bool AuthorizeRequest(HttpRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}
			string text = request.Query["connectionData"];
			if (!string.IsNullOrEmpty(text))
			{
				IEnumerable<ClientHubInfo> enumerable = base.JsonSerializer.Parse<IEnumerable<ClientHubInfo>>(text);
				if (enumerable != null && enumerable.Any())
				{
					Dictionary<string, HubDescriptor> dictionary = new Dictionary<string, HubDescriptor>(StringComparer.OrdinalIgnoreCase);
					foreach (ClientHubInfo item in enumerable)
					{
						if (dictionary.ContainsKey(item.Name))
						{
							throw new InvalidOperationException(Resources.Error_DuplicateHubNamesInConnectionData);
						}
						HubDescriptor hubDescriptor = _manager.EnsureHub(item.Name, _counters.ErrorsHubResolutionTotal, _counters.ErrorsHubResolutionPerSec, _counters.ErrorsAllTotal, _counters.ErrorsAllPerSec);
						if (_pipelineInvoker.AuthorizeConnect(hubDescriptor, request))
						{
							dictionary.Add(hubDescriptor.Name, hubDescriptor);
						}
					}
					_hubs.AddRange(dictionary.Values);
					return _hubs.Count > 0;
				}
			}
			return base.AuthorizeRequest(request);
		}

		protected override Task OnReceived(HttpRequest request, string connectionId, string data)
		{
			HubRequest hubRequest = _requestParser.Parse(data, _serializer);
			HubDescriptor hubDescriptor = _manager.EnsureHub(hubRequest.Hub, _counters.ErrorsHubInvocationTotal, _counters.ErrorsHubInvocationPerSec, _counters.ErrorsAllTotal, _counters.ErrorsAllPerSec);
			IJsonValue[] parameterValues = hubRequest.ParameterValues;
			MethodDescriptor methodDescriptor = _manager.GetHubMethod(hubDescriptor.Name, hubRequest.Method, parameterValues);
			if (methodDescriptor == null)
			{
				IEnumerable<MethodDescriptor> hubMethods = _manager.GetHubMethods(hubDescriptor.Name, (MethodDescriptor m) => m.Name == hubRequest.Method);
				methodDescriptor = new NullMethodDescriptor(hubDescriptor, hubRequest.Method, hubMethods);
			}
			IHub hub = CreateHub(request, hubDescriptor, connectionId, true);
			return InvokeHubPipeline(hub, parameterValues, methodDescriptor, hubRequest).ContinueWithPreservedCulture(delegate
			{
				hub.Dispose();
			}, TaskContinuationOptions.ExecuteSynchronously);
		}

		private Task InvokeHubPipeline(IHub hub, IJsonValue[] parameterValues, MethodDescriptor methodDescriptor, HubRequest hubRequest)
		{
			HubInvocationProgress progress = GetProgressInstance(methodDescriptor, (object value) => SendProgressUpdate(hub.Context.ConnectionId, value, hubRequest), Logger);
			Task<object> task2;
			try
			{
				IList<object> list = _binder.ResolveMethodParameters(methodDescriptor, parameterValues);
				if (progress != null)
				{
					list = list.Concat(new HubInvocationProgress[1]
					{
						progress
					}).ToList();
				}
				HubInvokerContext context = new HubInvokerContext(hub, methodDescriptor, list);
				task2 = _pipelineInvoker.Invoke(context);
			}
			catch (Exception e)
			{
				task2 = TaskAsyncHelper.FromError<object>(e);
			}
			return task2.ContinueWithPreservedCulture(delegate(Task<object> task)
			{
				if (progress != null)
				{
					progress.SetComplete();
				}
				if (task.IsFaulted)
				{
					return ProcessResponse(null, hubRequest, task.Exception);
				}
				return task.IsCanceled ? ProcessResponse(null, hubRequest, new OperationCanceledException()) : ProcessResponse(task.Result, hubRequest, null);
			}).FastUnwrap();
		}

		private static HubInvocationProgress GetProgressInstance(MethodDescriptor methodDescriptor, Func<object, Task> sendProgressFunc, ILogger logger)
		{
			HubInvocationProgress result = null;
			if ((object)methodDescriptor.ProgressReportingType != null)
			{
				result = HubInvocationProgress.Create(methodDescriptor.ProgressReportingType, sendProgressFunc, logger);
			}
			return result;
		}

		public override Task ProcessRequestCore(HttpContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			string text = context.Request.LocalPath().TrimEnd(new char[1]
			{
				'/'
			});
			int num = -1;
			if (text.EndsWith("/hubs", StringComparison.OrdinalIgnoreCase))
			{
				num = "/hubs".Length;
			}
			else if (text.EndsWith("/js", StringComparison.OrdinalIgnoreCase))
			{
				num = "/js".Length;
			}
			if (num != -1)
			{
				string serviceUrl = text.Substring(0, text.Length - num);
				context.Response.ContentType = JsonUtility.JavaScriptMimeType;
				return context.Response.End(_proxyGenerator.GenerateProxy(serviceUrl));
			}
			return base.ProcessRequestCore(context);
		}

		internal static Task Connect(IHub hub)
		{
			return hub.OnConnected();
		}

		internal static Task Reconnect(IHub hub)
		{
			return hub.OnReconnected();
		}

		internal static Task Disconnect(IHub hub, bool stopCalled)
		{
			return hub.OnDisconnected(stopCalled);
		}

		internal static Task<object> Incoming(IHubIncomingInvokerContext context)
		{
			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
			try
			{
				object obj = context.MethodDescriptor.Invoker(context.Hub, context.Args.ToArray());
				Type returnType = context.MethodDescriptor.ReturnType;
				if (TypeExtensions.IsAssignableFrom(typeof(Task), returnType))
				{
					Task task = (Task)obj;
					if (!returnType.GetTypeInfo().IsGenericType)
					{
						task.ContinueWith(taskCompletionSource);
					}
					else
					{
						Type type = TypeExtensions.GetGenericArguments(returnType).Single();
						Type type2 = typeof(Task<>).MakeGenericType(type);
						ParameterExpression parameterExpression = Expression.Parameter(typeof(object));
						Expression.Lambda<Action<object>>(Expression.Call(_continueWithMethod.MakeGenericMethod(type), Expression.Convert(parameterExpression, type2), Expression.Constant(taskCompletionSource)), new ParameterExpression[1]
						{
							parameterExpression
						}).Compile()(obj);
					}
				}
				else
				{
					taskCompletionSource.TrySetResult(obj);
				}
			}
			catch (Exception e)
			{
				taskCompletionSource.TrySetUnwrappedException(e);
			}
			return taskCompletionSource.Task;
		}

		internal static Task Outgoing(IHubOutgoingInvokerContext context)
		{
			ConnectionMessage connectionMessage = context.GetConnectionMessage();
			return context.Connection.Send(connectionMessage);
		}

		protected override Task OnConnected(HttpRequest request, string connectionId)
		{
			return ExecuteHubEvent(request, connectionId, (IHub hub) => _pipelineInvoker.Connect(hub));
		}

		protected override Task OnReconnected(HttpRequest request, string connectionId)
		{
			return ExecuteHubEvent(request, connectionId, (IHub hub) => _pipelineInvoker.Reconnect(hub));
		}

		protected override IList<string> OnRejoiningGroups(HttpRequest request, IList<string> groups, string connectionId)
		{
			return _hubs.Select(delegate(HubDescriptor hubDescriptor)
			{
				string groupPrefix = hubDescriptor.Name + ".";
				List<string> groups2 = (from g in groups
					where g.StartsWith(groupPrefix, StringComparison.OrdinalIgnoreCase)
					select g.Substring(groupPrefix.Length)).ToList();
				return from g in _pipelineInvoker.RejoiningGroups(hubDescriptor, request, groups2)
					select groupPrefix + g;
			}).SelectMany((IEnumerable<string> groupsToRejoin) => groupsToRejoin).ToList();
		}

		protected override Task OnDisconnected(HttpRequest request, string connectionId, bool stopCalled)
		{
			return ExecuteHubEvent(request, connectionId, (IHub hub) => _pipelineInvoker.Disconnect(hub, stopCalled));
		}

		protected override IList<string> GetSignals(string userId, string connectionId)
		{
			return _hubs.SelectMany(delegate(HubDescriptor info)
			{
				List<string> list = new List<string>
				{
					PrefixHelper.GetHubName(info.Name),
					PrefixHelper.GetHubConnectionId(info.CreateQualifiedName(connectionId))
				};
				if (!string.IsNullOrEmpty(userId))
				{
					list.Add(PrefixHelper.GetHubUserId(info.CreateQualifiedName(userId)));
				}
				return list;
			}).Concat(new string[1]
			{
				PrefixHelper.GetConnectionId(connectionId)
			}).ToList();
		}

		private Task ExecuteHubEvent(HttpRequest request, string connectionId, Func<IHub, Task> action)
		{
			List<IHub> hubs = GetHubs(request, connectionId).ToList();
			Task[] array = hubs.Select((IHub instance) => action(instance).OrEmpty().Catch(Logger)).ToArray();
			if (array.Length == 0)
			{
				DisposeHubs(hubs);
				return TaskAsyncHelper.Empty;
			}
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
			Task.Factory.ContinueWhenAll(array, delegate(Task[] tasks)
			{
				DisposeHubs(hubs);
				Task task = tasks.FirstOrDefault((Task t) => t.IsFaulted);
				if (task != null)
				{
					tcs.SetUnwrappedException(task.Exception);
				}
				else if (tasks.Any((Task t) => t.IsCanceled))
				{
					tcs.SetCanceled();
				}
				else
				{
					tcs.SetResult(null);
				}
			});
			return tcs.Task;
		}

		private IHub CreateHub(HttpRequest request, HubDescriptor descriptor, string connectionId, bool throwIfFailedToCreate = false)
		{
			try
			{
				IHub hub = _manager.ResolveHub(descriptor.Name);
				if (hub != null)
				{
					hub.Context = new HubCallerContext(request, connectionId);
					hub.Clients = new HubConnectionContext(_pipelineInvoker, base.Connection, descriptor.Name, connectionId);
					hub.Groups = new GroupManager(base.Connection, PrefixHelper.GetHubGroupName(descriptor.Name));
				}
				return hub;
			}
			catch (Exception ex)
			{
				Logger.LogInformation($"Error creating Hub {descriptor.Name}. {ex.Message}");
				if (throwIfFailedToCreate)
				{
					throw;
				}
				return null;
			}
		}

		private IEnumerable<IHub> GetHubs(HttpRequest request, string connectionId)
		{
			return from descriptor in _hubs
				select CreateHub(request, descriptor, connectionId) into hub
				where hub != null
				select hub;
		}

		private static void DisposeHubs(IEnumerable<IHub> hubs)
		{
			foreach (IHub hub in hubs)
			{
				hub.Dispose();
			}
		}

		private Task SendProgressUpdate(string connectionId, object value, HubRequest request)
		{
			HubResponse value2 = new HubResponse
			{
				Progress = new
				{
					I = request.Id,
					D = value
				},
				Id = "P|" + request.Id
			};
			return base.Connection.Send(connectionId, value2);
		}

		private Task ProcessResponse(object result, HubRequest request, Exception error)
		{
			HubResponse hubResponse = new HubResponse
			{
				Result = result,
				Id = request.Id
			};
			if (error != null)
			{
				_counters.ErrorsHubInvocationTotal.Increment();
				_counters.ErrorsHubInvocationPerSec.Increment();
				_counters.ErrorsAllTotal.Increment();
				_counters.ErrorsAllPerSec.Increment();
				HubException ex = error.InnerException as HubException;
				if (_enableDetailedErrors || ex != null)
				{
					Exception ex2 = error.InnerException ?? error;
					hubResponse.StackTrace = (_isDebuggingEnabled ? ex2.StackTrace : null);
					hubResponse.Error = ex2.Message;
					if (ex != null)
					{
						hubResponse.IsHubException = true;
						hubResponse.ErrorData = ex.ErrorData;
					}
				}
				else
				{
					hubResponse.Error = string.Format(CultureInfo.CurrentCulture, Resources.Error_HubInvocationFailed, request.Hub, request.Method);
				}
			}
			return base.Transport.Send(hubResponse);
		}

		private static void ContinueWith<T>(Task<T> task, TaskCompletionSource<object> tcs)
		{
			if (task.IsCompleted)
			{
				ContinueSync(task, tcs);
			}
			else
			{
				ContinueAsync(task, tcs);
			}
		}

		private static void ContinueSync<T>(Task<T> task, TaskCompletionSource<object> tcs)
		{
			if (task.IsFaulted)
			{
				tcs.TrySetUnwrappedException(task.Exception);
			}
			else if (task.IsCanceled)
			{
				tcs.TrySetCanceled();
			}
			else
			{
				tcs.TrySetResult(task.Result);
			}
		}

		private static void ContinueAsync<T>(Task<T> task, TaskCompletionSource<object> tcs)
		{
			task.ContinueWithPreservedCulture(delegate(Task<T> t)
			{
				if (t.IsFaulted)
				{
					tcs.TrySetUnwrappedException(t.Exception);
				}
				else if (t.IsCanceled)
				{
					tcs.TrySetCanceled();
				}
				else
				{
					tcs.TrySetResult(t.Result);
				}
			});
		}
	}
}
