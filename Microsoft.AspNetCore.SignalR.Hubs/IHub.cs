using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IHub : IDisposable
	{
		HubCallerContext Context
		{
			get;
			set;
		}

		IHubCallerConnectionContext<dynamic> Clients
		{
			get;
			set;
		}

		IGroupManager Groups
		{
			get;
			set;
		}

		Task OnConnected();

		Task OnReconnected();

		Task OnDisconnected(bool stopCalled);
	}
}
