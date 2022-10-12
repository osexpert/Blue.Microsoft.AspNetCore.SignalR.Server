using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	public abstract class TransportDisconnectBase : ITrackingConnection, IDisposable
	{
		private readonly HttpContext _context;

		private readonly ITransportHeartbeat _heartbeat;

		private ILogger _logger;

		private int _timedOut;

		private readonly IPerformanceCounterManager _counters;

		private int _ended;

		private TransportConnectionStates _state;

		protected string _lastMessageId;

		internal static readonly Func<Task> _emptyTaskFunc = () => TaskAsyncHelper.Empty;

		internal TaskCompletionSource<object> _connectTcs;

		private CancellationToken _connectionEndToken;

		private SafeCancellationTokenSource _connectionEndTokenSource;

		private Task _lastWriteTask = TaskAsyncHelper.Empty;

		private readonly CancellationToken _hostShutdownToken;

		private IDisposable _hostRegistration;

		private IDisposable _connectionEndRegistration;

		private readonly CancellationToken _requestAborted;

		internal HttpRequestLifeTime _requestLifeTime;

		protected IMemoryPool Pool
		{
			get;
			private set;
		}

		protected ILogger Logger => _logger;

		public string ConnectionId
		{
			get;
			set;
		}

		protected string LastMessageId => _lastMessageId;

		internal Microsoft.AspNetCore.SignalR.Infrastructure.TaskQueue WriteQueue
		{
			get;
			set;
		}

		public Func<bool, Task> Disconnected
		{
			get;
			set;
		}

		public virtual CancellationToken CancellationToken => _requestAborted;

		public virtual bool IsAlive
		{
			get
			{
				if (!CancellationToken.IsCancellationRequested && (_requestLifeTime == null || !_requestLifeTime.Task.IsCompleted) && !_lastWriteTask.IsCanceled)
				{
					return !_lastWriteTask.IsFaulted;
				}
				return false;
			}
		}

		public Task ConnectTask => _connectTcs.Task;

		protected CancellationToken ConnectionEndToken => _connectionEndToken;

		protected CancellationToken HostShutdownToken => _hostShutdownToken;

		public bool IsTimedOut => _timedOut == 1;

		public virtual bool SupportsKeepAlive => true;

		public virtual bool RequiresTimeout => false;

		public virtual TimeSpan DisconnectThreshold => TimeSpan.FromSeconds(5.0);

		protected bool IsConnectRequest => Context.Request.LocalPath().EndsWith("/connect", StringComparison.OrdinalIgnoreCase);

		protected bool IsSendRequest => Context.Request.LocalPath().EndsWith("/send", StringComparison.OrdinalIgnoreCase);

		protected bool IsAbortRequest => Context.Request.LocalPath().EndsWith("/abort", StringComparison.OrdinalIgnoreCase);

		protected virtual bool SuppressReconnect => false;

		protected ITransportConnection Connection
		{
			get;
			set;
		}

		protected HttpContext Context => _context;

		protected ITransportHeartbeat Heartbeat => _heartbeat;

		protected TransportDisconnectBase(HttpContext context, ITransportHeartbeat heartbeat, IPerformanceCounterManager performanceCounterManager, IApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory, IMemoryPool pool)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (heartbeat == null)
			{
				throw new ArgumentNullException("heartbeat");
			}
			if (performanceCounterManager == null)
			{
				throw new ArgumentNullException("performanceCounterManager");
			}
			if (applicationLifetime == null)
			{
				throw new ArgumentNullException("applicationLifetime");
			}
			if (loggerFactory == null)
			{
				throw new ArgumentNullException("loggerFactory");
			}
			Pool = pool;
			_context = context;
			_heartbeat = heartbeat;
			_counters = performanceCounterManager;
			_hostShutdownToken = applicationLifetime.ApplicationStopping;
			_requestAborted = context.RequestAborted;
			WriteQueue = new Microsoft.AspNetCore.SignalR.Infrastructure.TaskQueue();
			_logger = loggerFactory.CreateLogger(GetType().FullName);
		}

		protected virtual Task InitializeMessageId()
		{
			_lastMessageId = Context.Request.Query["messageId"];
			return TaskAsyncHelper.Empty;
		}

		public virtual Task<string> GetGroupsToken()
		{
			return TaskAsyncHelper.FromResult(Context.Request.Query["groupsToken"].ToString());
		}

		protected void IncrementErrors()
		{
			_counters.ErrorsTransportTotal.Increment();
			_counters.ErrorsTransportPerSec.Increment();
			_counters.ErrorsAllTotal.Increment();
			_counters.ErrorsAllPerSec.Increment();
		}

		public abstract void IncrementConnectionsCount();

		public abstract void DecrementConnectionsCount();

		public Task Disconnect()
		{
			return Abort(false);
		}

		protected Task Abort()
		{
			return Abort(true);
		}

		private Task Abort(bool clean)
		{
			if (clean)
			{
				ApplyState(TransportConnectionStates.Aborted);
			}
			else
			{
				ApplyState(TransportConnectionStates.Disconnected);
			}
			Logger.LogInformation("Abort(" + ConnectionId + ")");
			Heartbeat.RemoveConnection(this);
			End();
			return ((Disconnected != null) ? Disconnected(clean) : TaskAsyncHelper.Empty).Catch(delegate(AggregateException ex, object state)
			{
				OnDisconnectError(ex, state);
			}, Logger, Logger).Finally(delegate(object state)
			{
				((IPerformanceCounterManager)state).ConnectionsDisconnected.Increment();
			}, _counters);
		}

		public void ApplyState(TransportConnectionStates states)
		{
			_state |= states;
		}

		public void Timeout()
		{
			if (Interlocked.Exchange(ref _timedOut, 1) == 0)
			{
				Logger.LogInformation("Timeout(" + ConnectionId + ")");
				End();
			}
		}

		public virtual Task KeepAlive()
		{
			return TaskAsyncHelper.Empty;
		}

		public void End()
		{
			if (Interlocked.Exchange(ref _ended, 1) == 0)
			{
				Logger.LogInformation("End(" + ConnectionId + ")");
				if (_connectionEndTokenSource != null)
				{
					_connectionEndTokenSource.Cancel();
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_connectionEndTokenSource.Dispose();
				_connectionEndRegistration.Dispose();
				_hostRegistration.Dispose();
				ApplyState(TransportConnectionStates.Disposed);
			}
		}

		protected internal Task EnqueueOperation(Func<Task> writeAsync)
		{
			return EnqueueOperation((object state) => ((Func<Task>)state)(), writeAsync);
		}

		protected internal virtual Task EnqueueOperation(Func<object, Task> writeAsync, object state)
		{
			if (!IsAlive)
			{
				return TaskAsyncHelper.Empty;
			}
			return _lastWriteTask = WriteQueue.Enqueue(writeAsync, state);
		}

		protected virtual Task InitializePersistentState()
		{
			_requestLifeTime = new HttpRequestLifeTime(this, WriteQueue, Logger, ConnectionId);
			_connectTcs = new TaskCompletionSource<object>();
			_connectionEndTokenSource = new SafeCancellationTokenSource();
			_connectionEndToken = _connectionEndTokenSource.Token;
			_hostRegistration = _hostShutdownToken.SafeRegister(delegate(object state)
			{
				((SafeCancellationTokenSource)state).Cancel();
			}, _connectionEndTokenSource);
			_connectionEndRegistration = CancellationToken.SafeRegister(delegate(object state)
			{
				((HttpRequestLifeTime)state).Complete();
			}, _requestLifeTime);
			return InitializeMessageId();
		}

		private static void OnDisconnectError(AggregateException ex, object state)
		{
			((ILogger)state).LogError("Failed to raise disconnect: " + ex.GetBaseException());
		}
	}
}
