using System;

namespace Microsoft.AspNetCore.SignalR.Configuration
{
	internal static class TransportOptionsExtensions
	{
		public const int MissedTimeoutsBeforeClientReconnect = 2;

		public const int HeartBeatsPerKeepAlive = 2;

		public const int HeartBeatsPerDisconnectTimeout = 6;

		public static TimeSpan? KeepAliveTimeout(this TransportOptions options)
		{
			if (options.KeepAlive.HasValue)
			{
				return TimeSpan.FromTicks(options.KeepAlive.Value.Ticks * 2);
			}
			return null;
		}

		public static TimeSpan HeartbeatInterval(this TransportOptions options)
		{
			if (options.KeepAlive.HasValue)
			{
				return TimeSpan.FromTicks(options.KeepAlive.Value.Ticks / 2);
			}
			return TimeSpan.FromTicks(options.DisconnectTimeout.Ticks / 6);
		}

		public static TimeSpan TopicTtl(this TransportOptions options)
		{
			TimeSpan timeSpan = options.KeepAliveTimeout() ?? TimeSpan.Zero;
			return TimeSpan.FromTicks((options.DisconnectTimeout.Ticks + timeSpan.Ticks) * 2);
		}
	}
}
