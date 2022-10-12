namespace Microsoft.AspNetCore.SignalR.Hubs
{
	internal static class HubOutgoingInvokerContextExtensions
	{
		public static ConnectionMessage GetConnectionMessage(this IHubOutgoingInvokerContext context)
		{
			if (string.IsNullOrEmpty(context.Signal))
			{
				return new ConnectionMessage(context.Signals, context.Invocation, context.ExcludedSignals);
			}
			return new ConnectionMessage(context.Signal, context.Invocation, context.ExcludedSignals);
		}
	}
}
