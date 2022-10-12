using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

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
			if (string.IsNullOrEmpty(base.Context.get_Request().get_ContentType()))
			{
				base.Context.get_Request().set_ContentType(FormContentType);
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
			}
			else if (IsAbortRequest)
			{
				await Connection.Abort(ConnectionId).PreserveCulture();
			}
			else
			{
				await InitializePersistentState().PreserveCulture();
				await ProcessReceiveRequest(connection).PreserveCulture();
			}
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
			return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
		}

		protected void OnError(Exception ex)
		{
			IncrementErrors();
			_transportLifetime.Complete(ex);
		}

		protected virtual async Task ProcessSendRequest()
		{
			string arg = StringValues.op_Implicit((await Context.get_Request().ReadFormAsync(default(CancellationToken)).PreserveCulture()).get_Item("data"));
			if (Received != null)
			{
				await Received(arg).PreserveCulture();
			}
		}

		private Task ProcessReceiveRequest(ITransportConnection connection)
		{
			Func<Task> initialize2 = null;
			ITrackingConnection oldConnection = base.Heartbeat.AddOrUpdateConnection(this);
			bool flag = oldConnection == null;
			if (base.IsConnectRequest)
			{
				if (_protocolResolver.SupportsDelayedStart(base.Context.get_Request()))
				{
					initialize2 = (() => connection.Initialize(base.ConnectionId));
				}
				else
				{
					Func<Task> connected;
					if (flag)
					{
						connected = (Connected ?? TransportDisconnectBase._emptyTaskFunc);
						_counters.ConnectionsConnected.Increment();
					}
					else
					{
						connected = (() => oldConnection.ConnectTask);
					}
					Func<Task> initialize = () => connected().Then((ITransportConnection conn, string id) => conn.Initialize(id), connection, base.ConnectionId);
				}
			}
			else if (!SuppressReconnect)
			{
				initialize2 = Reconnected;
				_counters.ConnectionsReconnected.Increment();
			}
			initialize2 = (initialize2 ?? TransportDisconnectBase._emptyTaskFunc);
			Func<Task> initialize3 = () => initialize2().ContinueWith(_connectTcs);
			return ProcessMessages(connection, initialize3);
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
			LoggerExtensions.LogDebug(foreverTransportContext.Transport.Logger, "Cancel(" + foreverTransportContext.Transport.ConnectionId + ")", Array.Empty<object>());
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
					return Abort().Then(() => Microsoft.AspNetCore.SignalR.TaskAsyncHelper.False);
				}
			}
			if (response.Terminal)
			{
				_transportLifetime.Complete();
				return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.False;
			}
			return Send(response).Then(() => Microsoft.AspNetCore.SignalR.TaskAsyncHelper.True);
		}

		private static Task PerformSend(object state)
		{
			ForeverTransportContext foreverTransportContext = (ForeverTransportContext)state;
			if (!foreverTransportContext.Transport.IsAlive)
			{
				return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
			}
			foreverTransportContext.Transport.Context.get_Response().set_ContentType(JsonUtility.JsonMimeType);
			using (BinaryMemoryPoolTextWriter binaryMemoryPoolTextWriter = new BinaryMemoryPoolTextWriter(foreverTransportContext.Transport.Pool))
			{
				foreverTransportContext.Transport.JsonSerializer.Serialize(foreverTransportContext.State, binaryMemoryPoolTextWriter);
				binaryMemoryPoolTextWriter.Flush();
				foreverTransportContext.Transport.Context.get_Response().Write(binaryMemoryPoolTextWriter.Buffer);
			}
			return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
		}
	}
}
