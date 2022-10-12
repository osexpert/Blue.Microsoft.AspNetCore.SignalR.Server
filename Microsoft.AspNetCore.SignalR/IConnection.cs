using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR
{
	public interface IConnection
	{
		string DefaultSignal
		{
			get;
		}

		Task Send(ConnectionMessage message);
	}
}
