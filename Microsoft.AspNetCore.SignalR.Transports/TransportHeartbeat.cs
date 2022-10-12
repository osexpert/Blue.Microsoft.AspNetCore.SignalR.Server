using Microsoft.AspNetCore.SignalR.Configuration;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
			_transportOptions = optionsAccessor.get_Value().Transports;
			_counters = counters;
			_logger = LoggerFactoryExtensions.CreateLogger<TransportHeartbeat>(loggerFactory);
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
				LoggerExtensions.LogDebug(Logger, $"Connection {old.Connection.ConnectionId} exists. Closing previous connection.", Array.Empty<object>());
				old.Connection.ApplyState(TransportConnectionStates.Replaced);
				old.Connection.End();
				isNewConnection = false;
				oldConnection = old.Connection;
				return newMetadata;
			});
			if (isNewConnection)
			{
				LoggerExtensions.LogInformation(Logger, $"Connection {connection.ConnectionId} is New.", Array.Empty<object>());
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
			if (_connections.TryRemove(connection.ConnectionId, out ConnectionMetadata _))
			{
				connection.DecrementConnectionsCount();
				lock (_counterLock)
				{
					_counters.ConnectionsCurrent.RawValue = _connections.Count;
				}
				connection.ApplyState(TransportConnectionStates.Removed);
				LoggerExtensions.LogInformation(Logger, $"Removing connection {connection.ConnectionId}", Array.Empty<object>());
			}
		}

		public void MarkConnection(ITrackingConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			if (connection.IsAlive && _connections.TryGetValue(connection.ConnectionId, out ConnectionMetadata value))
			{
				value.LastMarked = DateTime.UtcNow;
			}
		}

		public IList<ITrackingConnection> GetConnections()
		{
			return (from metadata in _connections.Values
			select metadata.Connection).ToList();
		}

		private void Beat(object state)
		{
			if (Interlocked.Exchange(ref _running, 1) == 1)
			{
				LoggerExtensions.LogDebug(Logger, "Timer handler took longer than current interval", Array.Empty<object>());
			}
			else
			{
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
						}
						else
						{
							LoggerExtensions.LogDebug(Logger, value.Connection.ConnectionId + " is dead", Array.Empty<object>());
							CheckDisconnect(value);
						}
					}
				}
				catch (Exception ex)
				{
					LoggerExtensions.LogError(Logger, "SignalR error during transport heart beat on background thread: {0}", new object[1]
					{
						ex
					});
				}
				finally
				{
					Interlocked.Exchange(ref _running, 0);
				}
			}
		}

		private void CheckTimeoutAndKeepAlive(ConnectionMetadata metadata)
		{
			if (RaiseTimeout(metadata))
			{
				metadata.Connection.Timeout();
			}
			else
			{
				if (RaiseKeepAlive(metadata))
				{
					LoggerExtensions.LogDebug(Logger, "KeepAlive(" + metadata.Connection.ConnectionId + ")", Array.Empty<object>());
					metadata.Connection.KeepAlive().Catch(delegate(AggregateException ex, object state)
					{
						OnKeepAliveError(ex, state);
					}, Logger, Logger);
				}
				MarkConnection(metadata.Connection);
			}
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
				LoggerExtensions.LogError(Logger, $"Raising Disconnect failed: {arg}", Array.Empty<object>());
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
			if (disposing)
			{
				if (_timer != null)
				{
					_timer.Dispose();
				}
				LoggerExtensions.LogInformation(Logger, "Dispose(). Closing all connections", Array.Empty<object>());
				foreach (KeyValuePair<string, ConnectionMetadata> connection in _connections)
				{
					if (_connections.TryGetValue(connection.Key, out ConnectionMetadata value))
					{
						value.Connection.End();
					}
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private static void OnKeepAliveError(AggregateException ex, object state)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			LoggerExtensions.LogError(state, "Failed to send keep alive: " + ex.GetBaseException(), Array.Empty<object>());
		}
	}
}
