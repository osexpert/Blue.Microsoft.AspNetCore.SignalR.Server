using System;
using System.Threading;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal class DisposableAction : IDisposable
	{
		public static readonly Microsoft.AspNetCore.SignalR.Infrastructure.DisposableAction Empty = new Microsoft.AspNetCore.SignalR.Infrastructure.DisposableAction(delegate
		{
		});

		private Action<object> _action;

		private readonly object _state;

		public DisposableAction(Action action)
			: this(delegate(object state)
			{
				((Action)state)();
			}, action)
		{
		}

		public DisposableAction(Action<object> action, object state)
		{
			_action = action;
			_state = state;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Interlocked.Exchange(ref _action, delegate
				{
				})(_state);
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
