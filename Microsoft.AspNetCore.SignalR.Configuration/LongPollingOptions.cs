using System;

namespace Microsoft.AspNetCore.SignalR.Configuration
{
	public class LongPollingOptions
	{
		public TimeSpan PollDelay
		{
			get;
			set;
		}

		public TimeSpan PollTimeout
		{
			get;
			set;
		}

		public LongPollingOptions()
		{
			PollDelay = TimeSpan.Zero;
			PollTimeout = TimeSpan.FromSeconds(110.0);
		}
	}
}
