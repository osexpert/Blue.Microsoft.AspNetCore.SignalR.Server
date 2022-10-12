using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR.Configuration;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Json;
using Microsoft.AspNetCore.SignalR.Messaging;
using Microsoft.AspNetCore.SignalR.Transports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.SignalR
{
	public abstract class PersistentConnection
	{
		private const string WebSocketsTransportName = "webSockets";

		private const string PingJsonPayload = "{ \"Response\": \"pong\" }";

		private const string StartJsonPayload = "{ \"Response\": \"started\" }";

		private static readonly char[] SplitChars = new char[1]
		{
			':'
		};

		private static readonly ProtocolResolver _protocolResolver = new ProtocolResolver();

		private SignalROptions _options;

		private ITransportManager _transportManager;

		protected virtual ILogger Logger => LoggerFactory.CreateLogger<PersistentConnection>();

		protected IProtectedData ProtectedData
		{
			get;
			private set;
		}

		protected IMessageBus MessageBus
		{
			get;
			private set;
		}

		protected JsonSerializer JsonSerializer
		{
			get;
			private set;
		}

		protected IAckHandler AckHandler
		{
			get;
			private set;
		}

		protected ILoggerFactory LoggerFactory
		{
			get;
			private set;
		}

		protected IPerformanceCounterManager Counters
		{
			get;
			private set;
		}

		protected ITransport Transport
		{
			get;
			private set;
		}

		protected IUserIdProvider UserIdProvider
		{
			get;
			private set;
		}

		protected IMemoryPool Pool
		{
			get;
			set;
		}

		public IConnection Connection
		{
			get;
			private set;
		}

		public IConnectionGroupManager Groups
		{
			get;
			private set;
		}

		private string DefaultSignal => PrefixHelper.GetPersistentConnectionName(DefaultSignalRaw);

		private string DefaultSignalRaw => GetType().FullName;

		internal virtual string GroupPrefix => "pcg-";

		public virtual void Initialize(IServiceProvider serviceProvider)
		{
			MessageBus = serviceProvider.GetRequiredService<IMessageBus>();
			JsonSerializer = serviceProvider.GetRequiredService<JsonSerializer>();
			LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
			Counters = serviceProvider.GetRequiredService<IPerformanceCounterManager>();
			AckHandler = serviceProvider.GetRequiredService<IAckHandler>();
			ProtectedData = serviceProvider.GetRequiredService<IProtectedData>();
			UserIdProvider = serviceProvider.GetRequiredService<IUserIdProvider>();
			Pool = serviceProvider.GetRequiredService<IMemoryPool>();
			_options = serviceProvider.GetRequiredService<IOptions<SignalROptions>>().Value;
			_transportManager = serviceProvider.GetRequiredService<ITransportManager>();
			serviceProvider.GetRequiredService<AckSubscriber>();
		}

		public bool Authorize(HttpRequest request)
		{
			return AuthorizeRequest(request);
		}

		public Task ProcessRequest(HttpContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			HttpResponse response = context.Response;
			context.Response.Headers["X-Content-Type-Options"] = "nosniff";
			if (AuthorizeRequest(context.Request))
			{
				return ProcessRequestCore(context);
			}
			if (context.User != null && context.User.Identity.IsAuthenticated)
			{
				response.StatusCode = 403;
			}
			else
			{
				response.StatusCode = 401;
			}
			return TaskAsyncHelper.Empty;
		}

		public virtual async Task ProcessRequestCore(HttpContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (IsNegotiationRequest(context.Request))
			{
				await ProcessNegotiationRequest(context).PreserveCulture();
				return;
			}
			if (IsPingRequest(context.Request))
			{
				await ProcessPingRequest(context).PreserveCulture();
				return;
			}
			Transport = GetTransport(context);
			if (Transport == null)
			{
				await FailResponse(context.Response, string.Format(CultureInfo.CurrentCulture, Resources.Error_ProtocolErrorUnknownTransport)).PreserveCulture();
				return;
			}
			string connectionToken = context.Request.Query["connectionToken"];
			if (string.IsNullOrEmpty(connectionToken))
			{
				await FailResponse(context.Response, string.Format(CultureInfo.CurrentCulture, Resources.Error_ProtocolErrorMissingConnectionToken)).PreserveCulture();
				return;
			}
			if (!TryGetConnectionId(context, connectionToken, out var connectionId, out var message, out var statusCode))
			{
				await FailResponse(context.Response, message, statusCode).PreserveCulture();
				return;
			}
			Transport.ConnectionId = connectionId;
			string userId = UserIdProvider.GetUserId(context.Request);
			string groupsToken = await Transport.GetGroupsToken().PreserveCulture();
			IList<string> signals = GetSignals(userId, connectionId);
			IList<string> groups = AppendGroupPrefixes(context, connectionId, groupsToken);
			Connection connection = (Connection)(Connection = CreateConnection(connectionId, signals, groups));
			string persistentConnectionGroupName = PrefixHelper.GetPersistentConnectionGroupName(DefaultSignalRaw);
			Groups = new GroupManager(connection, persistentConnectionGroupName);
			if (IsStartRequest(context.Request))
			{
				await ProcessStartRequest(context, connectionId).PreserveCulture();
				return;
			}
			Transport.Connected = () => TaskAsyncHelper.FromMethod(() => OnConnected(context.Request, connectionId).OrEmpty());
			Transport.Reconnected = () => TaskAsyncHelper.FromMethod(() => OnReconnected(context.Request, connectionId).OrEmpty());
			Transport.Received = delegate(string data)
			{
				Counters.ConnectionMessagesSentTotal.Increment();
				Counters.ConnectionMessagesSentPerSec.Increment();
				return TaskAsyncHelper.FromMethod(() => OnReceived(context.Request, connectionId, data).OrEmpty());
			};
			Transport.Disconnected = (bool clean) => TaskAsyncHelper.FromMethod(() => OnDisconnected(context.Request, connectionId, clean).OrEmpty());
			await Transport.ProcessRequest(connection).OrEmpty().Catch(Logger, Counters.ErrorsAllTotal, Counters.ErrorsAllPerSec)
				.PreserveCulture();
		}

		internal bool TryGetConnectionId(HttpContext context, string connectionToken, out string connectionId, out string message, out int statusCode)
		{
			string text = null;
			connectionId = null;
			message = null;
			statusCode = 400;
			try
			{
				text = ProtectedData.Unprotect(connectionToken, "SignalR.ConnectionToken");
			}
			catch (Exception arg)
			{
				Logger.LogInformation($"Failed to process connectionToken {connectionToken}: {arg}");
			}
			if (string.IsNullOrEmpty(text))
			{
				message = string.Format(CultureInfo.CurrentCulture, Resources.Error_ConnectionIdIncorrectFormat);
				return false;
			}
			string[] array = text.Split(SplitChars, 2);
			connectionId = array[0];
			string a = ((array.Length > 1) ? array[1] : string.Empty);
			string userIdentity = GetUserIdentity(context);
			if (!string.Equals(a, userIdentity, StringComparison.OrdinalIgnoreCase))
			{
				message = string.Format(CultureInfo.CurrentCulture, Resources.Error_UnrecognizedUserIdentity);
				statusCode = 403;
				return false;
			}
			return true;
		}

		internal IList<string> VerifyGroups(string connectionId, string groupsToken)
		{
			if (string.IsNullOrEmpty(groupsToken))
			{
				return Microsoft.AspNetCore.SignalR.Infrastructure.ListHelper<string>.Empty;
			}
			string text = null;
			try
			{
				text = ProtectedData.Unprotect(groupsToken, "SignalR.Groups.v1.1");
			}
			catch (Exception arg)
			{
				Logger.LogInformation($"Failed to process groupsToken {groupsToken}: {arg}");
			}
			if (string.IsNullOrEmpty(text))
			{
				return Microsoft.AspNetCore.SignalR.Infrastructure.ListHelper<string>.Empty;
			}
			string[] array = text.Split(SplitChars, 2);
			string a = array[0];
			string json = ((array.Length > 1) ? array[1] : string.Empty);
			if (!string.Equals(a, connectionId, StringComparison.OrdinalIgnoreCase))
			{
				return Microsoft.AspNetCore.SignalR.Infrastructure.ListHelper<string>.Empty;
			}
			return JsonSerializer.Parse<string[]>(json);
		}

		private IList<string> AppendGroupPrefixes(HttpContext context, string connectionId, string groupsToken)
		{
			return (from g in OnRejoiningGroups(context.Request, VerifyGroups(connectionId, groupsToken), connectionId)
				select GroupPrefix + g).ToList();
		}

		private Connection CreateConnection(string connectionId, IList<string> signals, IList<string> groups)
		{
			return new Connection(MessageBus, JsonSerializer, DefaultSignal, connectionId, signals, groups, LoggerFactory, AckHandler, Counters, ProtectedData, Pool);
		}

		private IList<string> GetDefaultSignals(string userId, string connectionId)
		{
			return new string[2]
			{
				DefaultSignal,
				PrefixHelper.GetConnectionId(connectionId)
			};
		}

		protected virtual IList<string> GetSignals(string userId, string connectionId)
		{
			return GetDefaultSignals(userId, connectionId);
		}

		protected virtual bool AuthorizeRequest(HttpRequest request)
		{
			return true;
		}

		protected virtual IList<string> OnRejoiningGroups(HttpRequest request, IList<string> groups, string connectionId)
		{
			return groups;
		}

		protected virtual Task OnConnected(HttpRequest request, string connectionId)
		{
			return TaskAsyncHelper.Empty;
		}

		protected virtual Task OnReconnected(HttpRequest request, string connectionId)
		{
			return TaskAsyncHelper.Empty;
		}

		protected virtual Task OnReceived(HttpRequest request, string connectionId, string data)
		{
			return TaskAsyncHelper.Empty;
		}

		protected virtual Task OnDisconnected(HttpRequest request, string connectionId, bool stopCalled)
		{
			return TaskAsyncHelper.Empty;
		}

		private static Task ProcessPingRequest(HttpContext context)
		{
			return SendJsonResponse(context, "{ \"Response\": \"pong\" }");
		}

		private Task ProcessNegotiationRequest(HttpContext context)
		{
			TimeSpan? timeSpan = _options.Transports.KeepAliveTimeout();
			string text = Guid.NewGuid().ToString("d");
			string data = text + ":" + GetUserIdentity(context);
			var value = new
			{
				Url = context.Request.LocalPath().Replace("/negotiate", ""),
				ConnectionToken = ProtectedData.Protect(data, "SignalR.ConnectionToken"),
				ConnectionId = text,
				KeepAliveTimeout = (timeSpan.HasValue ? new double?(timeSpan.Value.TotalSeconds) : null),
				DisconnectTimeout = _options.Transports.DisconnectTimeout.TotalSeconds,
				ConnectionTimeout = _options.Transports.LongPolling.PollTimeout.TotalSeconds,
				TryWebSockets = (_transportManager.SupportsTransport("webSockets") && context.Features.Get<IHttpWebSocketFeature>() != null),
				ProtocolVersion = _protocolResolver.Resolve(context.Request).ToString(),
				TransportConnectTimeout = _options.Transports.TransportConnectTimeout.TotalSeconds,
				LongPollDelay = _options.Transports.LongPolling.PollDelay.TotalSeconds
			};
			return SendJsonResponse(context, JsonSerializer.Stringify(value));
		}

		private async Task ProcessStartRequest(HttpContext context, string connectionId)
		{
			await OnConnected(context.Request, connectionId).OrEmpty().PreserveCulture();
			await SendJsonResponse(context, "{ \"Response\": \"started\" }").PreserveCulture();
			Counters.ConnectionsConnected.Increment();
		}

		private static Task SendJsonResponse(HttpContext context, string jsonPayload)
		{
			StringValues values = context.Request.Query["callback"];
			if (string.IsNullOrEmpty(values))
			{
				context.Response.ContentType = JsonUtility.JsonMimeType;
				return context.Response.End(jsonPayload);
			}
			string data = JsonUtility.CreateJsonpCallback(values, jsonPayload);
			context.Response.ContentType = JsonUtility.JavaScriptMimeType;
			return context.Response.End(data);
		}

		private static string GetUserIdentity(HttpContext context)
		{
			if (context.User != null && context.User.Identity.IsAuthenticated)
			{
				return context.User.Identity.Name ?? string.Empty;
			}
			return string.Empty;
		}

		private static Task FailResponse(HttpResponse response, string message, int statusCode = 400)
		{
			response.StatusCode = statusCode;
			return response.End(message);
		}

		private static bool IsNegotiationRequest(HttpRequest request)
		{
			return request.LocalPath().EndsWith("/negotiate", StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsStartRequest(HttpRequest request)
		{
			return request.LocalPath().EndsWith("/start", StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsPingRequest(HttpRequest request)
		{
			return request.LocalPath().EndsWith("/ping", StringComparison.OrdinalIgnoreCase);
		}

		private ITransport GetTransport(HttpContext context)
		{
			return _transportManager.GetTransport(context);
		}
	}
}
