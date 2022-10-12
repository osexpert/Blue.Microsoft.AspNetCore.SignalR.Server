namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class ConnectionIdProxy : SignalProxy
	{
		public ConnectionIdProxy(IConnection connection, IHubPipelineInvoker invoker, string signal, string hubName, params string[] exclude)
			: base(connection, invoker, signal, hubName, "hc-", exclude)
		{
		}
	}
}
