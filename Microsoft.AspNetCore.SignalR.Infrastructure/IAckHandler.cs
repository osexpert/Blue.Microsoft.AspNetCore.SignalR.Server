using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public interface IAckHandler
	{
		Task CreateAck(string id);

		bool TriggerAck(string id);
	}
}
