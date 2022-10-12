using System;
using System.Threading;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal class Disposer : IDisposable
	{
		private static readonly object _disposedSentinel = new object();

		private object _disposable;

		public void Set(IDisposable disposable)
		{
			if (disposable == null)
			{
				throw new ArgumentNullException("disposable");
			}
			object obj = Interlocked.CompareExchange(ref _disposable, disposable, null);
			if (obj != null && obj == _disposedSentinel)
			{
				disposable.Dispose();
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				(Interlocked.Exchange(ref _disposable, _disposedSentinel) as IDisposable)?.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
