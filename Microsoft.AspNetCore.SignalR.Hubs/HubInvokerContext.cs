using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	internal class HubInvokerContext : IHubIncomingInvokerContext
	{
		public IHub Hub
		{
			get;
			private set;
		}

		public MethodDescriptor MethodDescriptor
		{
			get;
			private set;
		}

		public IList<object> Args
		{
			get;
			private set;
		}

		public HubInvokerContext(IHub hub, MethodDescriptor methodDescriptor, IList<object> args)
		{
			Hub = hub;
			MethodDescriptor = methodDescriptor;
			Args = args;
		}
	}
}
