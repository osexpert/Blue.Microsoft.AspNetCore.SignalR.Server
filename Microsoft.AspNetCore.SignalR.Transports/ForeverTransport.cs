using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	public abstract class ForeverTransport : TransportDisconnectBase, ITransport
	{
		private class ForeverTransportContext
		{
			public object State;

			public ForeverTransport Transport;

			public ForeverTransportContext(ForeverTransport foreverTransport, object state)
			{
				State = state;
				Transport = foreverTransport;
			}
		}

		private class SubscriptionDisposerContext
		{
			private readonly Microsoft.AspNetCore.SignalR.Infrastructure.Disposer _disposer;

			private readonly IDisposable _supscription;

			public SubscriptionDisposerContext(Microsoft.AspNetCore.SignalR.Infrastructure.Disposer disposer, IDisposable subscription)
			{
				_disposer = disposer;
				_supscription = subscription;
			}

			public void Set()
			{
				_disposer.Set(_supscription);
			}
		}

		internal class RequestLifetime
		{
			private readonly HttpRequestLifeTime _lifetime;

			private readonly ForeverTransport _transport;

			public RequestLifetime(ForeverTransport transport, HttpRequestLifeTime lifetime)
			{
				_lifetime = lifetime;
				_transport = transport;
			}

			public void Complete()
			{
				Complete(null);
			}

			public void Complete(Exception error)
			{
				_lifetime.Complete(error);
				_transport.Dispose();
				if (_transport.AfterRequestEnd != null)
				{
					_transport.AfterRequestEnd(error);
				}
			}
		}

		protected static readonly string FormContentType = "application/x-www-form-urlencoded";

		private static readonly ProtocolResolver _protocolResolver = new ProtocolResolver();

		private readonly IPerformanceCounterManager _counters;

		private readonly JsonSerializer _jsonSerializer;

		private IDisposable _busRegistration;

		internal RequestLifetime _transportLifetime;

		internal Action AfterReceive;

		internal Action BeforeCancellationTokenCallbackRegistered;

		internal Action BeforeReceive;

		internal Action<Exception> AfterRequestEnd;

		protected virtual int MaxMessages => 10;

		protected JsonSerializer JsonSerializer => _jsonSerializer;

		public Func<string, Task> Received
		{
			get;
			set;
		}

		public Func<Task> Connected
		{
			get;
			set;
		}

		public Func<Task> Reconnected
		{
			get;
			set;
		}

		protected ForeverTransport(HttpContext context, JsonSerializer jsonSerializer, ITransportHeartbeat heartbeat, IPerformanceCounterManager performanceCounterManager, IApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory, IMemoryPool pool)
			: base(context, heartbeat, performanceCounterManager, applicationLifetime, loggerFactory, pool)
		{
			_jsonSerializer = jsonSerializer;
			_counters = performanceCounterManager;
		}

		protected void EnsureFormContentType()
		{
			if (string.IsNullOrEmpty(base.Context.Request.ContentType))
			{
				base.Context.Request.ContentType = FormContentType;
			}
		}

		protected virtual void OnSending(string payload)
		{
			base.Heartbeat.MarkConnection(this);
		}

		protected virtual void OnSendingResponse(PersistentResponse response)
		{
			base.Heartbeat.MarkConnection(this);
		}

		protected override async Task InitializePersistentState()
		{
			await base.InitializePersistentState().PreserveCulture();
			_transportLifetime = new RequestLifetime(this, _requestLifeTime);
		}

		protected async Task ProcessRequestCore(ITransportConnection connection)
		{
			EnsureFormContentType();
			Connection = connection;
			if (IsSendRequest)
			{
				await ProcessSendRequest().PreserveCulture();
				return;
			}
			if (IsAbortRequest)
			{
				await Connection.Abort(ConnectionId).PreserveCulture();
				return;
			}
			await InitializePersistentState().PreserveCulture();
			await ProcessReceiveRequest(connection).PreserveCulture();
		}

		public virtual Task ProcessRequest(ITransportConnection connection)
		{
			return ProcessRequestCore(connection);
		}

		public abstract Task Send(PersistentResponse response);

		public virtual Task Send(object value)
		{
			ForeverTransportContext state2 = new ForeverTransportContext(this, value);
			return EnqueueOperation((object state) => PerformSend(state), state2);
		}

		protected internal virtual Task InitializeResponse(ITransportConnection connection)
		{
			return TaskAsyncHelper.Empty;
		}

		protected void OnError(Exception ex)
		{
			IncrementErrors();
			_transportLifetime.Complete(ex);
		}

		protected virtual async Task ProcessSendRequest()
		{
			string arg = (await Context.Request.ReadFormAsync().PreserveCulture())["data"];
			if (Received != null)
			{
				await Received(arg).PreserveCulture();
			}
		}

		private Task ProcessReceiveRequest(ITransportConnection connection)
		{
			Func<Task> initialize = null;
			ITrackingConnection oldConnection = base.Heartbeat.AddOrUpdateConnection(this);
			bool flag = oldConnection == null;
			if (base.IsConnectRequest)
			{
				if (_protocolResolver.SupportsDelayedStart(base.Context.Request))
				{
					initialize = () => connection.Initialize(base.ConnectionId);
				}
				else
				{
					Func<Task> connected;
					if (flag)
					{
						connected = Connected ?? TransportDisconnectBase._emptyTaskFunc;
						_counters.ConnectionsConnected.Increment();
					}
					else
					{
						connected = () => oldConnection.ConnectTask;
					}
					initialize = () => connected().Then((ITransportConnection conn, string id) => conn.Initialize(id), connection, base.ConnectionId);
				}
			}
			else if (!SuppressReconnect)
			{
				initialize = Reconnected;
				_counters.ConnectionsReconnected.Increment();
			}
			initialize = initialize ?? TransportDisconnectBase._emptyTaskFunc;
			Func<Task> initialize2 = () => initialize().ContinueWith(_connectTcs);
			return ProcessMessages(connection, initialize2);
		}

		private Task ProcessMessages(ITransportConnection connection, Func<Task> initialize)
		{
			Microsoft.AspNetCore.SignalR.Infrastructure.Disposer disposer = new Microsoft.AspNetCore.SignalR.Infrastructure.Disposer();
			if (BeforeCancellationTokenCallbackRegistered != null)
			{
				BeforeCancellationTokenCallbackRegistered();
			}
			ForeverTransportContext state2 = new ForeverTransportContext(this, disposer);
			_busRegistration = base.ConnectionEndToken.SafeRegister(delegate(object state)
			{
				Cancel(state);
			}, state2);
			if (BeforeReceive != null)
			{
				BeforeReceive();
			}
			try
			{
				EnqueueOperation((object state) => InitializeResponse((ITransportConnection)state), connection).Catch(delegate(AggregateException ex, object state)
				{
					((ForeverTransport)state).OnError(ex);
				}, this, base.Logger);
				IDisposable subscription = connection.Receive(base.LastMessageId, (PersistentResponse response, object state) => ((ForeverTransport)state).OnMessageReceived(response), MaxMessages, this);
				if (AfterReceive != null)
				{
					AfterReceive();
				}
				initialize().Catch(delegate(AggregateException ex, object state)
				{
					((ForeverTransport)state).OnError(ex);
				}, this, base.Logger).Finally(delegate(object state)
				{
					((SubscriptionDisposerContext)state).Set();
				}, new SubscriptionDisposerContext(disposer, subscription));
			}
			catch (Exception error)
			{
				_transportLifetime.Complete(error);
			}
			return _requestLifeTime.Task;
		}

		private static void Cancel(object state)
		{
			ForeverTransportContext foreverTransportContext = (ForeverTransportContext)state;
			foreverTransportContext.Transport.Logger.LogDebug("Cancel(" + foreverTransportContext.Transport.ConnectionId + ")");
			((IDisposable)foreverTransportContext.State).Dispose();
		}

		protected virtual Task<bool> OnMessageReceived(PersistentResponse response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}
			response.Reconnect = base.HostShutdownToken.IsCancellationRequested;
			if (base.IsTimedOut || response.Aborted)
			{
				_busRegistration.Dispose();
				if (response.Aborted)
				{
					return Abort().Then(() => TaskAsyncHelper.False);
				}
			}
			if (response.Terminal)
			{
				_transportLifetime.Complete();
				return TaskAsyncHelper.False;
			}
			return Send(response).Then(() => TaskAsyncHelper.True);
		}

		private static async Task PerformSend(object state)
		{
			ForeverTransportContext foreverTransportContext = (ForeverTransportContext)state;
			if (!foreverTransportContext.Transport.IsAlive)
			{
				return;// TaskAsyncHelper.Empty;
			}
			foreverTransportContext.Transport.Context.Response.ContentType = JsonUtility.JsonMimeType;
			using (BinaryMemoryPoolTextWriter binaryMemoryPoolTextWriter = new BinaryMemoryPoolTextWriter(foreverTransportContext.Transport.Pool))
			{
				foreverTransportContext.Transport.JsonSerializer.Serialize(foreverTransportContext.State, binaryMemoryPoolTextWriter);
				binaryMemoryPoolTextWriter.Flush();
				await foreverTransportContext.Transport.Context.Response.WriteAsync(binaryMemoryPoolTextWriter.Buffer);
			}
			return;// TaskAsyncHelper.Empty;
		}
	}
}
