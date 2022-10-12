using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR
{
	public interface IConnectionGroupManager : IGroupManager
	{
		Task Send(string groupName, object value, params string[] excludeConnectionIds);

		Task Send(IList<string> groupNames, object value, params string[] excludeConnectionIds);
	}
}
