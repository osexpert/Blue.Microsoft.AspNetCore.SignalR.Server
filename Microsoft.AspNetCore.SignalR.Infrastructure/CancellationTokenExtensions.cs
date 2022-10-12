using System;
using System.Threading;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal static class CancellationTokenExtensions
	{
		private delegate CancellationTokenRegistration RegisterDelegate(ref CancellationToken token, Action<object> callback, object state);

		private class DiposeCancellationState
		{
			private readonly CancellationCallbackWrapper _callbackWrapper;

			private readonly CancellationTokenRegistration _registration;

			public DiposeCancellationState(CancellationCallbackWrapper callbackWrapper, CancellationTokenRegistration registration)
			{
				_callbackWrapper = callbackWrapper;
				_registration = registration;
			}

			public void TryDispose()
			{
				if (_callbackWrapper.TrySetInvoked())
				{
					_registration.Dispose();
				}
			}
		}

		private class CancellationCallbackWrapper
		{
			private readonly Action<object> _callback;

			private readonly object _state;

			private int _callbackInvoked;

			public CancellationCallbackWrapper(Action<object> callback, object state)
			{
				_callback = callback;
				_state = state;
			}

			public bool TrySetInvoked()
			{
				return Interlocked.Exchange(ref _callbackInvoked, 1) == 0;
			}

			public void TryInvoke()
			{
				if (TrySetInvoked())
				{
					_callback(_state);
				}
			}
		}

		private static readonly RegisterDelegate _tokenRegister = ResolveRegisterDelegate();

		public static IDisposable SafeRegister(this CancellationToken cancellationToken, Action<object> callback, object state)
		{
			CancellationCallbackWrapper cancellationCallbackWrapper = new CancellationCallbackWrapper(callback, state);
			CancellationTokenRegistration registration = _tokenRegister(ref cancellationToken, delegate(object s)
			{
				InvokeCallback(s);
			}, cancellationCallbackWrapper);
			DiposeCancellationState state2 = new DiposeCancellationState(cancellationCallbackWrapper, registration);
			return new Microsoft.AspNetCore.SignalR.Infrastructure.DisposableAction(delegate(object s)
			{
				Dispose(s);
			}, state2);
		}

		private static void InvokeCallback(object state)
		{
			((CancellationCallbackWrapper)state).TryInvoke();
		}

		private static void Dispose(object state)
		{
			((DiposeCancellationState)state).TryDispose();
		}

		private static RegisterDelegate ResolveRegisterDelegate()
		{
			return delegate(ref CancellationToken token, Action<object> callback, object state)
			{
				return token.Register(callback, state);
			};
		}
	}
}
