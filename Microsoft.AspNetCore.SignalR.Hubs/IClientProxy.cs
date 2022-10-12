using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IClientProxy
	{
		Task Invoke(string method, params object[] args);
	}
}
