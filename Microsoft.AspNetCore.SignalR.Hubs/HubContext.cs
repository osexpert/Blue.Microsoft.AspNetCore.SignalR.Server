using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	internal class HubContext : IHubContext
	{
		public IHubConnectionContext<dynamic> Clients
		{
			get;
			private set;
		}

		public IGroupManager Groups
		{
			get;
			private set;
		}

		public HubContext(IConnection connection, IHubPipelineInvoker invoker, string hubName)
		{
			Clients = new HubConnectionContextBase(connection, invoker, hubName);
			Groups = new GroupManager(connection, PrefixHelper.GetHubGroupName(hubName));
		}
	}
	internal class HubContext<THub, TClient> : IHubContext<THub, TClient> where THub : IHub where TClient : class
	{
		public IHubConnectionContext<TClient> Clients
		{
			get;
			private set;
		}

		public IGroupManager Groups
		{
			get;
			private set;
		}

		public HubContext(IHubContext dynamicContext)
		{
			TypedClientBuilder<TClient>.Validate();
			Clients = new TypedHubConnectionContext<TClient>(dynamicContext.Clients);
			Groups = dynamicContext.Groups;
		}
	}
}
