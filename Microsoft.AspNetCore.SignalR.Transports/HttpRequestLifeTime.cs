using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	internal class HttpRequestLifeTime
	{
		private class LifetimeContext
		{
			private readonly TaskCompletionSource<object> _lifetimeTcs;

			private readonly Exception _error;

			private readonly TransportDisconnectBase _transport;

			public LifetimeContext(TransportDisconnectBase transport, TaskCompletionSource<object> lifeTimetcs, Exception error)
			{
				_transport = transport;
				_lifetimeTcs = lifeTimetcs;
				_error = error;
			}

			public void Complete()
			{
				_transport.ApplyState(TransportConnectionStates.HttpRequestEnded);
				if (_error != null)
				{
					_lifetimeTcs.TrySetUnwrappedException(_error);
				}
				else
				{
					_lifetimeTcs.TrySetResult(null);
				}
			}
		}

		private readonly TaskCompletionSource<object> _lifetimeTcs = new TaskCompletionSource<object>();

		private readonly TransportDisconnectBase _transport;

		private readonly Microsoft.AspNetCore.SignalR.Infrastructure.TaskQueue _writeQueue;

		private readonly ILogger _logger;

		private readonly string _connectionId;

		public Task Task => _lifetimeTcs.Task;

		public HttpRequestLifeTime(TransportDisconnectBase transport, Microsoft.AspNetCore.SignalR.Infrastructure.TaskQueue writeQueue, ILogger logger, string connectionId)
		{
			_transport = transport;
			_logger = logger;
			_connectionId = connectionId;
			_writeQueue = writeQueue;
		}

		public void Complete()
		{
			Complete(null);
		}

		public void Complete(Exception error)
		{
			_logger.LogDebug("DrainWrites(" + _connectionId + ")");
			LifetimeContext state2 = new LifetimeContext(_transport, _lifetimeTcs, error);
			_transport.ApplyState(TransportConnectionStates.QueueDrained);
			_writeQueue.Drain().Catch(_logger).Finally(delegate(object state)
			{
				((LifetimeContext)state).Complete();
			}, state2);
			if (error != null)
			{
				_logger.LogError("CompleteRequest (" + _connectionId + ") failed: " + error.GetBaseException());
			}
			else
			{
				_logger.LogInformation("CompleteRequest (" + _connectionId + ")");
			}
		}
	}
}
