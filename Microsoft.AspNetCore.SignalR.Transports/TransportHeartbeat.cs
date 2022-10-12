using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.SignalR.Configuration;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	public class TransportHeartbeat : ITransportHeartbeat, IDisposable
	{
		private class ConnectionMetadata
		{
			public ITrackingConnection Connection
			{
				get;
				set;
			}

			public DateTime LastMarked
			{
				get;
				set;
			}

			public DateTime Initial
			{
				get;
				set;
			}

			public ConnectionMetadata(ITrackingConnection connection)
			{
				Connection = connection;
				Initial = DateTime.UtcNow;
				LastMarked = DateTime.UtcNow;
			}
		}

		private readonly ConcurrentDictionary<string, ConnectionMetadata> _connections = new ConcurrentDictionary<string, ConnectionMetadata>();

		private readonly Timer _timer;

		private readonly TransportOptions _transportOptions;

		private readonly ILogger _logger;

		private readonly IPerformanceCounterManager _counters;

		private readonly object _counterLock = new object();

		private int _running;

		private ulong _heartbeatCount;

		private ILogger Logger => _logger;

		public TransportHeartbeat(IOptions<SignalROptions> optionsAccessor, IPerformanceCounterManager counters, ILoggerFactory loggerFactory)
		{
			_transportOptions = optionsAccessor.Value.Transports;
			_counters = counters;
			_logger = loggerFactory.CreateLogger<TransportHeartbeat>();
			_timer = new Timer(Beat, null, _transportOptions.HeartbeatInterval(), _transportOptions.HeartbeatInterval());
		}

		public ITrackingConnection AddOrUpdateConnection(ITrackingConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			ConnectionMetadata newMetadata = new ConnectionMetadata(connection);
			bool isNewConnection = true;
			ITrackingConnection oldConnection = null;
			_connections.AddOrUpdate(connection.ConnectionId, newMetadata, delegate(string key, ConnectionMetadata old)
			{
				Logger.LogDebug($"Connection {old.Connection.ConnectionId} exists. Closing previous connection.");
				old.Connection.ApplyState(TransportConnectionStates.Replaced);
				old.Connection.End();
				isNewConnection = false;
				oldConnection = old.Connection;
				return newMetadata;
			});
			if (isNewConnection)
			{
				Logger.LogInformation($"Connection {connection.ConnectionId} is New.");
				connection.IncrementConnectionsCount();
			}
			lock (_counterLock)
			{
				_counters.ConnectionsCurrent.RawValue = _connections.Count;
			}
			newMetadata.Initial = DateTime.UtcNow;
			newMetadata.Connection.ApplyState(TransportConnectionStates.Added);
			return oldConnection;
		}

		public void RemoveConnection(ITrackingConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			if (_connections.TryRemove(connection.ConnectionId, out var _))
			{
				connection.DecrementConnectionsCount();
				lock (_counterLock)
				{
					_counters.ConnectionsCurrent.RawValue = _connections.Count;
				}
				connection.ApplyState(TransportConnectionStates.Removed);
				Logger.LogInformation($"Removing connection {connection.ConnectionId}");
			}
		}

		public void MarkConnection(ITrackingConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			if (connection.IsAlive && _connections.TryGetValue(connection.ConnectionId, out var value))
			{
				value.LastMarked = DateTime.UtcNow;
			}
		}

		public IList<ITrackingConnection> GetConnections()
		{
			return _connections.Values.Select((ConnectionMetadata metadata) => metadata.Connection).ToList();
		}

		private void Beat(object state)
		{
			if (Interlocked.Exchange(ref _running, 1) == 1)
			{
				Logger.LogDebug("Timer handler took longer than current interval");
				return;
			}
			lock (_counterLock)
			{
				_counters.ConnectionsCurrent.RawValue = _connections.Count;
			}
			try
			{
				_heartbeatCount++;
				foreach (ConnectionMetadata value in _connections.Values)
				{
					if (value.Connection.IsAlive)
					{
						CheckTimeoutAndKeepAlive(value);
						continue;
					}
					Logger.LogDebug(value.Connection.ConnectionId + " is dead");
					CheckDisconnect(value);
				}
			}
			catch (Exception ex)
			{
				Logger.LogError("SignalR error during transport heart beat on background thread: {0}", ex);
			}
			finally
			{
				Interlocked.Exchange(ref _running, 0);
			}
		}

		private void CheckTimeoutAndKeepAlive(ConnectionMetadata metadata)
		{
			if (RaiseTimeout(metadata))
			{
				metadata.Connection.Timeout();
				return;
			}
			if (RaiseKeepAlive(metadata))
			{
				Logger.LogDebug("KeepAlive(" + metadata.Connection.ConnectionId + ")");
				metadata.Connection.KeepAlive().Catch(delegate(AggregateException ex, object state)
				{
					OnKeepAliveError(ex, state);
				}, Logger, Logger);
			}
			MarkConnection(metadata.Connection);
		}

		private void CheckDisconnect(ConnectionMetadata metadata)
		{
			try
			{
				if (RaiseDisconnect(metadata))
				{
					RemoveConnection(metadata.Connection);
					metadata.Connection.Disconnect();
				}
			}
			catch (Exception arg)
			{
				Logger.LogError($"Raising Disconnect failed: {arg}");
			}
		}

		private bool RaiseDisconnect(ConnectionMetadata metadata)
		{
			TimeSpan t = DateTime.UtcNow - metadata.LastMarked;
			TimeSpan t2 = metadata.Connection.DisconnectThreshold + _transportOptions.DisconnectTimeout;
			return t >= t2;
		}

		private bool RaiseKeepAlive(ConnectionMetadata metadata)
		{
			if (!_transportOptions.KeepAlive.HasValue || !metadata.Connection.SupportsKeepAlive)
			{
				return false;
			}
			return _heartbeatCount % 2uL == 0;
		}

		private bool RaiseTimeout(ConnectionMetadata metadata)
		{
			if (metadata.Connection.IsTimedOut)
			{
				return false;
			}
			if (_transportOptions.KeepAlive.HasValue && !metadata.Connection.RequiresTimeout)
			{
				return false;
			}
			return DateTime.UtcNow - metadata.Initial >= _transportOptions.LongPolling.PollTimeout;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}
			if (_timer != null)
			{
				_timer.Dispose();
			}
			Logger.LogInformation("Dispose(). Closing all connections");
			foreach (KeyValuePair<string, ConnectionMetadata> connection in _connections)
			{
				if (_connections.TryGetValue(connection.Key, out var value))
				{
					value.Connection.End();
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private static void OnKeepAliveError(AggregateException ex, object state)
		{
			((ILogger)state).LogError("Failed to send keep alive: " + ex.GetBaseException());
		}
	}
}
