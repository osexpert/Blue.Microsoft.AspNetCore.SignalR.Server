namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal class PersistentConnectionContext : IPersistentConnectionContext
	{
		public IConnection Connection
		{
			get;
			private set;
		}

		public IConnectionGroupManager Groups
		{
			get;
			private set;
		}

		public PersistentConnectionContext(IConnection connection, IConnectionGroupManager groupManager)
		{
			Connection = connection;
			Groups = groupManager;
		}
	}
}
