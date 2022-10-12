using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class UserProxy : SignalProxy
	{
		public UserProxy(IConnection connection, IHubPipelineInvoker invoker, string signal, string hubName)
			: base(connection, invoker, signal, hubName, "hu-", Microsoft.AspNetCore.SignalR.Infrastructure.ListHelper<string>.Empty)
		{
		}
	}
}
