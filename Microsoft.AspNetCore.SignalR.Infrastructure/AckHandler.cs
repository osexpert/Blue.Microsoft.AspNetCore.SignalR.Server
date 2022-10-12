using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public class AckHandler : IAckHandler, IDisposable
	{
		private class AckInfo
		{
			public TaskCompletionSource<object> Tcs
			{
				get;
				private set;
			}

			public DateTime Created
			{
				get;
				private set;
			}

			public AckInfo()
			{
				Tcs = new TaskCompletionSource<object>();
				Created = DateTime.UtcNow;
			}
		}

		private readonly ConcurrentDictionary<string, AckInfo> _acks = new ConcurrentDictionary<string, AckInfo>();

		private readonly TimeSpan _ackThreshold;

		private Timer _timer;

		public AckHandler()
			: this(true, TimeSpan.FromSeconds(30.0), TimeSpan.FromSeconds(5.0))
		{
		}

		internal AckHandler(bool completeAcksOnTimeout, TimeSpan ackThreshold, TimeSpan ackInterval)
		{
			if (completeAcksOnTimeout)
			{
				_timer = new Timer(delegate
				{
					CheckAcks();
				}, null, ackInterval, ackInterval);
			}
			_ackThreshold = ackThreshold;
		}

		public Task CreateAck(string id)
		{
			return _acks.GetOrAdd(id, (string _) => new AckInfo()).Tcs.Task;
		}

		public bool TriggerAck(string id)
		{
			if (_acks.TryRemove(id, out var value))
			{
				value.Tcs.TrySetResult(null);
				return true;
			}
			return false;
		}

		private void CheckAcks()
		{
			foreach (KeyValuePair<string, AckInfo> ack in _acks)
			{
				if (DateTime.UtcNow - ack.Value.Created > _ackThreshold && _acks.TryRemove(ack.Key, out var value))
				{
					value.Tcs.TrySetCanceled();
				}
			}
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
			foreach (KeyValuePair<string, AckInfo> ack in _acks)
			{
				if (_acks.TryRemove(ack.Key, out var value))
				{
					value.Tcs.TrySetCanceled();
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
