using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IHubIncomingInvokerContext
	{
		IHub Hub
		{
			get;
		}

		MethodDescriptor MethodDescriptor
		{
			get;
		}

		IList<object> Args
		{
			get;
		}
	}
}
