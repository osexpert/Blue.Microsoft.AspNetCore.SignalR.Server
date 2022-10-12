using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IHubPipelineModule
	{
		Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(Func<IHubIncomingInvokerContext, Task<object>> invoke);

		Func<IHubOutgoingInvokerContext, Task> BuildOutgoing(Func<IHubOutgoingInvokerContext, Task> send);

		Func<IHub, Task> BuildConnect(Func<IHub, Task> connect);

		Func<IHub, Task> BuildReconnect(Func<IHub, Task> reconnect);

		Func<IHub, bool, Task> BuildDisconnect(Func<IHub, bool, Task> disconnect);

		Func<HubDescriptor, HttpRequest, bool> BuildAuthorizeConnect(Func<HubDescriptor, HttpRequest, bool> authorizeConnect);

		Func<HubDescriptor, HttpRequest, IList<string>, IList<string>> BuildRejoiningGroups(Func<HubDescriptor, HttpRequest, IList<string>, IList<string>> rejoiningGroups);
	}
}
