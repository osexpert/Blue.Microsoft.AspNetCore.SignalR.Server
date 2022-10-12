using System;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	[Flags]
	public enum TransportConnectionStates
	{
		None = 0x0,
		Added = 0x1,
		Removed = 0x2,
		Replaced = 0x4,
		QueueDrained = 0x8,
		HttpRequestEnded = 0x10,
		Disconnected = 0x20,
		Aborted = 0x40,
		Disposed = 0x10000
	}
}
