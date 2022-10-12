using System;
using System.Threading;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal class SafeCancellationTokenSource : IDisposable
	{
		private static class State
		{
			public const int Initial = 0;

			public const int Cancelling = 1;

			public const int Cancelled = 2;

			public const int Disposing = 3;

			public const int Disposed = 4;
		}

		private CancellationTokenSource _cts;

		private int _state;

		public CancellationToken Token
		{
			get;
			private set;
		}

		public SafeCancellationTokenSource()
		{
			_cts = new CancellationTokenSource();
			Token = _cts.Token;
		}

		public void Cancel(bool useNewThread = true)
		{
			if (Interlocked.CompareExchange(ref _state, 1, 0) == 0)
			{
				if (!useNewThread)
				{
					CancelCore();
				}
				else
				{
					ThreadPool.QueueUserWorkItem(delegate
					{
						CancelCore();
					});
				}
			}
		}

		private void CancelCore()
		{
			try
			{
				_cts.Cancel();
			}
			finally
			{
				if (Interlocked.CompareExchange(ref _state, 2, 1) == 3)
				{
					_cts.Dispose();
					Interlocked.Exchange(ref _state, 4);
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				switch (Interlocked.Exchange(ref _state, 3))
				{
				case 1:
				case 3:
					break;
				case 0:
				case 2:
					_cts.Dispose();
					Interlocked.Exchange(ref _state, 4);
					break;
				case 4:
					Interlocked.Exchange(ref _state, 4);
					break;
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
