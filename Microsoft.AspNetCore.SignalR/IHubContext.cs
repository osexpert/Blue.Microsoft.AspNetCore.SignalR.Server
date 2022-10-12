using Microsoft.AspNetCore.SignalR.Hubs;

namespace Microsoft.AspNetCore.SignalR
{
	public interface IHubContext
	{
		IHubConnectionContext<dynamic> Clients
		{
			get;
		}

		IGroupManager Groups
		{
			get;
		}
	}
	public interface IHubContext<THub> : IHubContext where THub : IHub
	{
	}
	public interface IHubContext<THub, TClient> where THub : IHub where TClient : class
	{
		IHubConnectionContext<TClient> Clients
		{
			get;
		}

		IGroupManager Groups
		{
			get;
		}
	}
}
