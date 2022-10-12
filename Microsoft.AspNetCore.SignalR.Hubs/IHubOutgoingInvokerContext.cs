using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IHubOutgoingInvokerContext
	{
		IConnection Connection
		{
			get;
		}

		ClientHubInvocation Invocation
		{
			get;
		}

		string Signal
		{
			get;
		}

		IList<string> Signals
		{
			get;
		}

		IList<string> ExcludedSignals
		{
			get;
		}
	}
}
