using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IAuthorizeHubConnection
	{
		bool AuthorizeHubConnection(HubDescriptor hubDescriptor, HttpRequest request);
	}
}
