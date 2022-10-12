using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public abstract class HubPipelineModule : IHubPipelineModule
	{
		public virtual Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(Func<IHubIncomingInvokerContext, Task<object>> invoke)
		{
			return async delegate(IHubIncomingInvokerContext context)
			{
				if (OnBeforeIncoming(context))
				{
					try
					{
						object result = await invoke(context).OrEmpty().PreserveCulture();
						return OnAfterIncoming(result, context);
					}
					catch (Exception ex)
					{
						ExceptionContext exceptionContext = new ExceptionContext(ex);
						OnIncomingError(exceptionContext, context);
						Exception error = exceptionContext.Error;
						if (error == ex)
						{
							throw;
						}
						if (error != null)
						{
							throw error;
						}
						return exceptionContext.Result;
					}
				}
				return null;
			};
		}

		public virtual Func<IHub, Task> BuildConnect(Func<IHub, Task> connect)
		{
			return (IHub hub) => OnBeforeConnect(hub) ? connect(hub).OrEmpty().Then(delegate(IHub h)
			{
				OnAfterConnect(h);
			}, hub) : TaskAsyncHelper.Empty;
		}

		public virtual Func<IHub, Task> BuildReconnect(Func<IHub, Task> reconnect)
		{
			return (IHub hub) => OnBeforeReconnect(hub) ? reconnect(hub).OrEmpty().Then(delegate(IHub h)
			{
				OnAfterReconnect(h);
			}, hub) : TaskAsyncHelper.Empty;
		}

		public virtual Func<IHub, bool, Task> BuildDisconnect(Func<IHub, bool, Task> disconnect)
		{
			return (IHub hub, bool stopCalled) => OnBeforeDisconnect(hub, stopCalled) ? disconnect(hub, stopCalled).OrEmpty().Then(delegate(IHub h, bool s)
			{
				OnAfterDisconnect(h, s);
			}, hub, stopCalled) : TaskAsyncHelper.Empty;
		}

		public virtual Func<HubDescriptor, HttpRequest, bool> BuildAuthorizeConnect(Func<HubDescriptor, HttpRequest, bool> authorizeConnect)
		{
			return (HubDescriptor hubDescriptor, HttpRequest request) => OnBeforeAuthorizeConnect(hubDescriptor, request) && authorizeConnect(hubDescriptor, request);
		}

		public virtual Func<HubDescriptor, HttpRequest, IList<string>, IList<string>> BuildRejoiningGroups(Func<HubDescriptor, HttpRequest, IList<string>, IList<string>> rejoiningGroups)
		{
			return rejoiningGroups;
		}

		public virtual Func<IHubOutgoingInvokerContext, Task> BuildOutgoing(Func<IHubOutgoingInvokerContext, Task> send)
		{
			return (IHubOutgoingInvokerContext context) => OnBeforeOutgoing(context) ? send(context).OrEmpty().Then(delegate(IHubOutgoingInvokerContext ctx)
			{
				OnAfterOutgoing(ctx);
			}, context) : TaskAsyncHelper.Empty;
		}

		protected virtual bool OnBeforeAuthorizeConnect(HubDescriptor hubDescriptor, HttpRequest request)
		{
			return true;
		}

		protected virtual bool OnBeforeConnect(IHub hub)
		{
			return true;
		}

		protected virtual void OnAfterConnect(IHub hub)
		{
		}

		protected virtual bool OnBeforeReconnect(IHub hub)
		{
			return true;
		}

		protected virtual void OnAfterReconnect(IHub hub)
		{
		}

		protected virtual bool OnBeforeOutgoing(IHubOutgoingInvokerContext context)
		{
			return true;
		}

		protected virtual void OnAfterOutgoing(IHubOutgoingInvokerContext context)
		{
		}

		protected virtual bool OnBeforeDisconnect(IHub hub, bool stopCalled)
		{
			return true;
		}

		protected virtual void OnAfterDisconnect(IHub hub, bool stopCalled)
		{
		}

		protected virtual bool OnBeforeIncoming(IHubIncomingInvokerContext context)
		{
			return true;
		}

		protected virtual object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
		{
			return result;
		}

		protected virtual void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
		{
		}
	}
}
