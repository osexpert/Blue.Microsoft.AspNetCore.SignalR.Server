using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class GroupProxy : SignalProxy
	{
		public GroupProxy(IConnection connection, IHubPipelineInvoker invoker, string signal, string hubName, IList<string> exclude)
			: base(connection, invoker, signal, hubName, "hg-", exclude)
		{
		}
	}
}
