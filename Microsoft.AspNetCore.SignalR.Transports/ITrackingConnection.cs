using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	public interface ITrackingConnection : IDisposable
	{
		string ConnectionId
		{
			get;
		}

		CancellationToken CancellationToken
		{
			get;
		}

		Task ConnectTask
		{
			get;
		}

		bool IsAlive
		{
			get;
		}

		bool IsTimedOut
		{
			get;
		}

		bool SupportsKeepAlive
		{
			get;
		}

		bool RequiresTimeout
		{
			get;
		}

		TimeSpan DisconnectThreshold
		{
			get;
		}

		void ApplyState(TransportConnectionStates states);

		Task Disconnect();

		void Timeout();

		Task KeepAlive();

		void IncrementConnectionsCount();

		void DecrementConnectionsCount();

		void End();
	}
}
