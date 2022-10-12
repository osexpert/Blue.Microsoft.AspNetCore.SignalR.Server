using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	public interface ITransportHeartbeat
	{
		ITrackingConnection AddOrUpdateConnection(ITrackingConnection connection);

		void MarkConnection(ITrackingConnection connection);

		void RemoveConnection(ITrackingConnection connection);

		IList<ITrackingConnection> GetConnections();
	}
}
