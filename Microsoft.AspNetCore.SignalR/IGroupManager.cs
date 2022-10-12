using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR
{
	public interface IGroupManager
	{
		Task Add(string connectionId, string groupName);

		Task Remove(string connectionId, string groupName);
	}
}
