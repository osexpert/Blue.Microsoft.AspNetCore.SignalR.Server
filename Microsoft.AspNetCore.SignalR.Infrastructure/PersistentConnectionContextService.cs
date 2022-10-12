namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public class PersistentConnectionContextService<TConnection> : IPersistentConnectionContext<TConnection>, IPersistentConnectionContext where TConnection : PersistentConnection
	{
		private readonly IPersistentConnectionContext _connectionContext;

		public IConnection Connection => _connectionContext.Connection;

		public IConnectionGroupManager Groups => _connectionContext.Groups;

		public PersistentConnectionContextService(IConnectionManager connectionManager)
		{
			_connectionContext = connectionManager.GetConnectionContext<TConnection>();
		}
	}
}
