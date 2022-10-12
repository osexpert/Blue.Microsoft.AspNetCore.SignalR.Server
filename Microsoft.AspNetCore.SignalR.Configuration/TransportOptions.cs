using System;

namespace Microsoft.AspNetCore.SignalR.Configuration
{
	public class TransportOptions
	{
		private static readonly TimeSpan _minimumKeepAlive = TimeSpan.FromSeconds(2.0);

		private const int _minimumKeepAlivesPerDisconnectTimeout = 3;

		private static readonly TimeSpan _minimumDisconnectTimeout = TimeSpan.FromTicks(_minimumKeepAlive.Ticks * 3);

		private bool _keepAliveConfigured;

		private TimeSpan? _keepAlive;

		private TimeSpan _disconnectTimeout;

		public TransportType EnabledTransports
		{
			get;
			set;
		}

		public TimeSpan TransportConnectTimeout
		{
			get;
			set;
		}

		public TimeSpan DisconnectTimeout
		{
			get
			{
				return _disconnectTimeout;
			}
			set
			{
				if (value < _minimumDisconnectTimeout)
				{
					throw new ArgumentOutOfRangeException("value", Resources.Error_DisconnectTimeoutMustBeAtLeastSixSeconds);
				}
				if (_keepAliveConfigured)
				{
					throw new InvalidOperationException(Resources.Error_DisconnectTimeoutCannotBeConfiguredAfterKeepAlive);
				}
				_disconnectTimeout = value;
				_keepAlive = TimeSpan.FromTicks(_disconnectTimeout.Ticks / 3);
			}
		}

		public TimeSpan? KeepAlive
		{
			get
			{
				return _keepAlive;
			}
			set
			{
				if (value < _minimumKeepAlive)
				{
					throw new ArgumentOutOfRangeException("value", Resources.Error_KeepAliveMustBeGreaterThanTwoSeconds);
				}
				if (value > TimeSpan.FromTicks(_disconnectTimeout.Ticks / 3))
				{
					throw new ArgumentOutOfRangeException("value", Resources.Error_KeepAliveMustBeNoMoreThanAThirdOfTheDisconnectTimeout);
				}
				_keepAlive = value;
				_keepAliveConfigured = true;
			}
		}

		public WebSocketOptions WebSockets
		{
			get;
			set;
		}

		public LongPollingOptions LongPolling
		{
			get;
			set;
		}

		public TransportOptions()
		{
			EnabledTransports = TransportType.All;
			TransportConnectTimeout = TimeSpan.FromSeconds(5.0);
			DisconnectTimeout = TimeSpan.FromSeconds(30.0);
			WebSockets = new WebSocketOptions();
			LongPolling = new LongPollingOptions();
		}
	}
}
