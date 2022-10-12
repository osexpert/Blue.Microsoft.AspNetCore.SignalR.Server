using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	public interface ITransportConnection
	{
		IDisposable Receive(string messageId, Func<PersistentResponse, object, Task<bool>> callback, int maxMessages, object state);

		Task Send(ConnectionMessage message);
	}
}
