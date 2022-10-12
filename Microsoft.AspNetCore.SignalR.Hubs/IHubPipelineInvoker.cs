using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IHubPipelineInvoker
	{
		Task<object> Invoke(IHubIncomingInvokerContext context);

		Task Send(IHubOutgoingInvokerContext context);

		Task Connect(IHub hub);

		Task Reconnect(IHub hub);

		Task Disconnect(IHub hub, bool stopCalled);

		bool AuthorizeConnect(HubDescriptor hubDescriptor, HttpRequest request);

		IList<string> RejoiningGroups(HubDescriptor hubDescriptor, HttpRequest request, IList<string> groups);
	}
}
