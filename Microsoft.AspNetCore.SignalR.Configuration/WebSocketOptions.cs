namespace Microsoft.AspNetCore.SignalR.Configuration
{
	public class WebSocketOptions
	{
		public int? MaxIncomingMessageSize
		{
			get;
			set;
		}

		public WebSocketOptions()
		{
			MaxIncomingMessageSize = 65536;
		}
	}
}
