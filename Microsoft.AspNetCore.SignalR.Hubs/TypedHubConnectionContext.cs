using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class TypedHubConnectionContext<TClient> : IHubConnectionContext<TClient> where TClient : class
	{
		private IHubConnectionContext<dynamic> _dynamicContext;

		public TClient All => TypedClientBuilder<TClient>.Build(_dynamicContext.All);

		public TypedHubConnectionContext(IHubConnectionContext<dynamic> dynamicContext)
		{
			_dynamicContext = dynamicContext;
		}

		public TClient AllExcept(params string[] excludeConnectionIds)
		{
			return TypedClientBuilder<TClient>.Build(_dynamicContext.AllExcept(excludeConnectionIds));
		}

		public TClient Client(string connectionId)
		{
			return TypedClientBuilder<TClient>.Build(_dynamicContext.Client(connectionId));
		}

		public TClient Clients(IList<string> connectionIds)
		{
			return TypedClientBuilder<TClient>.Build(_dynamicContext.Clients(connectionIds));
		}

		public TClient Group(string groupName, params string[] excludeConnectionIds)
		{
			return TypedClientBuilder<TClient>.Build(_dynamicContext.Group(groupName, excludeConnectionIds));
		}

		public TClient Groups(IList<string> groupNames, params string[] excludeConnectionIds)
		{
			return TypedClientBuilder<TClient>.Build(_dynamicContext.Groups(groupNames, excludeConnectionIds));
		}

		public TClient User(string userId)
		{
			return TypedClientBuilder<TClient>.Build(_dynamicContext.User(userId));
		}

		public TClient Users(IList<string> userIds)
		{
			return TypedClientBuilder<TClient>.Build(_dynamicContext.Users(userIds));
		}
	}
}
