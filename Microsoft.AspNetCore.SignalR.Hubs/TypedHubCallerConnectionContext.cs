using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class TypedHubCallerConnectionContext<TClient> : TypedHubConnectionContext<TClient>, IHubCallerConnectionContext<TClient>, IHubConnectionContext<TClient> where TClient : class
	{
		private IHubCallerConnectionContext<dynamic> _dynamicContext;

		public TClient Caller => (TClient)TypedClientBuilder<TClient>.Build(_dynamicContext.Caller);

		public TClient Others => (TClient)TypedClientBuilder<TClient>.Build(_dynamicContext.Others);

		public TypedHubCallerConnectionContext(IHubCallerConnectionContext<dynamic> dynamicContext)
			: base((IHubConnectionContext<dynamic>)dynamicContext)
		{
			_dynamicContext = dynamicContext;
		}

		public TClient OthersInGroup(string groupName)
		{
			return (TClient)TypedClientBuilder<TClient>.Build(_dynamicContext.OthersInGroup(groupName));
		}

		public TClient OthersInGroups(IList<string> groupNames)
		{
			return (TClient)TypedClientBuilder<TClient>.Build(_dynamicContext.OthersInGroups(groupNames));
		}
	}
}
