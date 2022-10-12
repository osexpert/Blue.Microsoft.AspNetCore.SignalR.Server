using System;

namespace Microsoft.AspNetCore.SignalR
{
	[Flags]
	public enum TransportType
	{
		All = 0x7,
		Streaming = 0x3,
		WebSockets = 0x1,
		ServerSentEvents = 0x2,
		LongPolling = 0x4
	}
}
