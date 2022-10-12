namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IAuthorizeHubMethodInvocation
	{
		bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod);
	}
}
