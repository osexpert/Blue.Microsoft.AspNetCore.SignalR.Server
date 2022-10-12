using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	public interface ITransport
	{
		Func<string, Task> Received
		{
			get;
			set;
		}

		Func<Task> Connected
		{
			get;
			set;
		}

		Func<Task> Reconnected
		{
			get;
			set;
		}

		Func<bool, Task> Disconnected
		{
			get;
			set;
		}

		string ConnectionId
		{
			get;
			set;
		}

		Task<string> GetGroupsToken();

		Task ProcessRequest(ITransportConnection connection);

		Task Send(object value);
	}
}
