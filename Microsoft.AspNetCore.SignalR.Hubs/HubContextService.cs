using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class HubContextService<THub> : IHubContext<THub>, IHubContext where THub : IHub
	{
		private readonly IHubContext _hubContext;

		public IHubConnectionContext<dynamic> Clients => _hubContext.Clients;

		public IGroupManager Groups => _hubContext.Groups;

		public HubContextService(IConnectionManager connectionManager)
		{
			_hubContext = connectionManager.GetHubContext<THub>();
		}
	}
	public class HubContextService<THub, TClient> : IHubContext<THub, TClient> where THub : IHub where TClient : class
	{
		private readonly IHubContext<THub, TClient> _hubContext;

		public IHubConnectionContext<TClient> Clients => _hubContext.Clients;

		public IGroupManager Groups => _hubContext.Groups;

		public HubContextService(IConnectionManager connectionManager)
		{
			_hubContext = connectionManager.GetHubContext<THub, TClient>();
		}
	}
}
