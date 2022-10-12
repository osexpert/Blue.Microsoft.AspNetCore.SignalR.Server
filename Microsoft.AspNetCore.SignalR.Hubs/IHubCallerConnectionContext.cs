using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IHubCallerConnectionContext<TClient> : IHubConnectionContext<TClient> where TClient : class
	{
		TClient Caller
		{
			get;
		}

		TClient Others
		{
			get;
		}

		TClient OthersInGroup(string groupName);

		TClient OthersInGroups(IList<string> groupNames);
	}
}
