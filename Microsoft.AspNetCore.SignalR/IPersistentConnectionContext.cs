namespace Microsoft.AspNetCore.SignalR
{
	public interface IPersistentConnectionContext
	{
		IConnection Connection
		{
			get;
		}

		IConnectionGroupManager Groups
		{
			get;
		}
	}
	public interface IPersistentConnectionContext<TConnection> : IPersistentConnectionContext where TConnection : PersistentConnection
	{
	}
}
