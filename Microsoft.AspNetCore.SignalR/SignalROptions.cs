using Microsoft.AspNetCore.SignalR.Configuration;

namespace Microsoft.AspNetCore.SignalR
{
	public class SignalROptions
	{
		public bool EnableJSONP
		{
			get;
			set;
		}

		public HubOptions Hubs
		{
			get;
			set;
		}

		public MessageBusOptions MessageBus
		{
			get;
			set;
		}

		public TransportOptions Transports
		{
			get;
			set;
		}

		public SignalROptions()
		{
			Hubs = new HubOptions();
			MessageBus = new MessageBusOptions();
			Transports = new TransportOptions();
		}
	}
}
