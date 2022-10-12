using Microsoft.AspNetCore.SignalR.Hubs;
using System;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public interface IConnectionManager
	{
		IHubContext GetHubContext<THub>() where THub : IHub;

		IHubContext GetHubContext(string hubName);

		IHubContext<THub, TClient> GetHubContext<THub, TClient>() where THub : IHub where TClient : class;

		IPersistentConnectionContext GetConnectionContext<TConnection>() where TConnection : PersistentConnection;

		IPersistentConnectionContext GetConnectionContext(Type type);
	}
}
