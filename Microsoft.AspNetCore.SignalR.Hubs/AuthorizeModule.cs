using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class AuthorizeModule : HubPipelineModule
	{
		private readonly IAuthorizeHubConnection _globalConnectionAuthorizer;

		private readonly IAuthorizeHubMethodInvocation _globalInvocationAuthorizer;

		private readonly ConcurrentDictionary<Type, IEnumerable<IAuthorizeHubConnection>> _connectionAuthorizersCache;

		private readonly ConcurrentDictionary<Type, IEnumerable<IAuthorizeHubMethodInvocation>> _classInvocationAuthorizersCache;

		private readonly ConcurrentDictionary<MethodDescriptor, IEnumerable<IAuthorizeHubMethodInvocation>> _methodInvocationAuthorizersCache;

		public AuthorizeModule()
			: this(null, null)
		{
		}

		public AuthorizeModule(IAuthorizeHubConnection globalConnectionAuthorizer, IAuthorizeHubMethodInvocation globalInvocationAuthorizer)
		{
			_globalConnectionAuthorizer = globalConnectionAuthorizer;
			_globalInvocationAuthorizer = globalInvocationAuthorizer;
			_connectionAuthorizersCache = new ConcurrentDictionary<Type, IEnumerable<IAuthorizeHubConnection>>();
			_classInvocationAuthorizersCache = new ConcurrentDictionary<Type, IEnumerable<IAuthorizeHubMethodInvocation>>();
			_methodInvocationAuthorizersCache = new ConcurrentDictionary<MethodDescriptor, IEnumerable<IAuthorizeHubMethodInvocation>>();
		}

		public override Func<HubDescriptor, HttpRequest, bool> BuildAuthorizeConnect(Func<HubDescriptor, HttpRequest, bool> authorizeConnect)
		{
			return base.BuildAuthorizeConnect(delegate(HubDescriptor hubDescriptor, HttpRequest request)
			{
				if (!authorizeConnect(hubDescriptor, request))
				{
					return false;
				}
				if (_globalConnectionAuthorizer != null && !_globalConnectionAuthorizer.AuthorizeHubConnection(hubDescriptor, request))
				{
					return false;
				}
				return _connectionAuthorizersCache.GetOrAdd(hubDescriptor.HubType, (Type hubType) => hubType.GetTypeInfo().GetCustomAttributes().OfType<IAuthorizeHubConnection>()).All((IAuthorizeHubConnection a) => a.AuthorizeHubConnection(hubDescriptor, request));
			});
		}

		public override Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(Func<IHubIncomingInvokerContext, Task<object>> invoke)
		{
			return base.BuildIncoming(delegate(IHubIncomingInvokerContext context)
			{
				if ((_globalInvocationAuthorizer == null || _globalInvocationAuthorizer.AuthorizeHubMethodInvocation(context, false)) && _classInvocationAuthorizersCache.GetOrAdd(context.Hub.GetType(), (Type hubType) => hubType.GetTypeInfo().GetCustomAttributes().OfType<IAuthorizeHubMethodInvocation>()).All((IAuthorizeHubMethodInvocation a) => a.AuthorizeHubMethodInvocation(context, false)))
				{
					if (context.MethodDescriptor is NullMethodDescriptor)
					{
						return invoke(context);
					}
					if (_methodInvocationAuthorizersCache.GetOrAdd(context.MethodDescriptor, (MethodDescriptor methodDescriptor) => methodDescriptor.Attributes.OfType<IAuthorizeHubMethodInvocation>()).All((IAuthorizeHubMethodInvocation a) => a.AuthorizeHubMethodInvocation(context, true)))
					{
						return invoke(context);
					}
				}
				return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.FromError<object>(new NotAuthorizedException(string.Format(CultureInfo.CurrentCulture, Resources.Error_CallerNotAuthorizedToInvokeMethodOn, context.MethodDescriptor.Name, context.MethodDescriptor.Hub.Name)));
			});
		}
	}
}
