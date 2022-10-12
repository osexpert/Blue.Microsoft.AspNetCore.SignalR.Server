namespace Microsoft.AspNetCore.SignalR.Configuration
{
	public class MessageBusOptions
	{
		public int MessageBufferSize
		{
			get;
			set;
		}

		public int MaxTopicsWithNoSubscriptions
		{
			get;
			set;
		}

		public MessageBusOptions()
		{
			MessageBufferSize = 1000;
			MaxTopicsWithNoSubscriptions = 1000;
		}
	}
}
