using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	public class LongPollingTransport : ForeverTransport, ITransport
	{
		private class LongPollingTransportContext
		{
			public object State;

			public LongPollingTransport Transport;

			public LongPollingTransportContext(LongPollingTransport transport, object state)
			{
				State = state;
				Transport = transport;
			}
		}

		private readonly TimeSpan _pollDelay;

		private readonly IPerformanceCounterManager _counters;

		private bool _responseSent;

		private static readonly ArraySegment<byte> _keepAlive = new ArraySegment<byte>(new byte[1]
		{
			32
		});

		public override TimeSpan DisconnectThreshold => _pollDelay;

		private bool IsJsonp => !string.IsNullOrEmpty(JsonpCallback);

		private string JsonpCallback => StringValues.op_Implicit(base.Context.get_Request().get_Query().get_Item("callback"));

		public override bool SupportsKeepAlive => !IsJsonp;

		public override bool RequiresTimeout => true;

		protected override int MaxMessages => 5000;

		protected override bool SuppressReconnect => !base.Context.get_Request().LocalPath().EndsWith("/reconnect", StringComparison.OrdinalIgnoreCase);

		public LongPollingTransport(HttpContext context, JsonSerializer jsonSerializer, ITransportHeartbeat heartbeat, IPerformanceCounterManager performanceCounterManager, IApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory, IOptions<SignalROptions> optionsAccessor, IMemoryPool pool)
			: base(context, jsonSerializer, heartbeat, performanceCounterManager, applicationLifetime, loggerFactory, pool)
		{
			_pollDelay = optionsAccessor.get_Value().Transports.LongPolling.PollDelay;
			_counters = performanceCounterManager;
		}

		protected override async Task InitializeMessageId()
		{
			_lastMessageId = StringValues.op_Implicit(Context.get_Request().get_Query().get_Item("messageId"));
			if (_lastMessageId == null && Context.get_Request().get_HasFormContentType())
			{
				_lastMessageId = StringValues.op_Implicit((await Context.get_Request().ReadFormAsync(default(CancellationToken)).PreserveCulture()).get_Item("messageId"));
			}
		}

		public override async Task<string> GetGroupsToken()
		{
			StringValues val = Context.get_Request().get_Query().get_Item("groupsToken");
			if (val.get_Count() == 0 && Context.get_Request().get_HasFormContentType())
			{
				val = (await Context.get_Request().ReadFormAsync(default(CancellationToken)).PreserveCulture()).get_Item("groupsToken");
			}
			return StringValues.op_Implicit(val);
		}

		public override Task KeepAlive()
		{
			return EnqueueOperation((object state) => PerformKeepAlive(state), this);
		}

		public override Task Send(PersistentResponse response)
		{
			base.Heartbeat.MarkConnection(this);
			AddTransportData(response);
			LongPollingTransportContext state2 = new LongPollingTransportContext(this, response);
			return EnqueueOperation((object state) => PerformPartialSend(state), state2);
		}

		public override Task Send(object value)
		{
			LongPollingTransportContext state2 = new LongPollingTransportContext(this, value);
			return EnqueueOperation((object state) => PerformCompleteSend(state), state2);
		}

		public override void IncrementConnectionsCount()
		{
			_counters.ConnectionsCurrentLongPolling.Increment();
		}

		public override void DecrementConnectionsCount()
		{
			_counters.ConnectionsCurrentLongPolling.Decrement();
		}

		protected override Task<bool> OnMessageReceived(PersistentResponse response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}
			response.Reconnect = base.HostShutdownToken.IsCancellationRequested;
			Task task = Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
			if (response.Aborted)
			{
				task = Abort();
			}
			if (response.Terminal)
			{
				if (!_responseSent)
				{
					return task.Then((LongPollingTransport transport, PersistentResponse resp) => transport.Send(resp), this, response).Then(delegate
					{
						_transportLifetime.Complete();
						return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.False;
					});
				}
				return task.Then(delegate
				{
					_transportLifetime.Complete();
					return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.False;
				});
			}
			_responseSent = true;
			return task.Then((LongPollingTransport transport, PersistentResponse resp) => transport.Send(resp), this, response).Then(() => Microsoft.AspNetCore.SignalR.TaskAsyncHelper.False);
		}

		protected internal override Task InitializeResponse(ITransportConnection connection)
		{
			return base.InitializeResponse(connection).Then((LongPollingTransport s) => WriteInit(s), this);
		}

		protected override async Task ProcessSendRequest()
		{
			if (string.IsNullOrEmpty(Context.get_Request().get_ContentType()))
			{
				Context.get_Request().set_ContentType(ForeverTransport.FormContentType);
			}
			string arg = StringValues.op_Implicit((await Context.get_Request().ReadFormAsync(default(CancellationToken)).PreserveCulture()).get_Item("data")) ?? StringValues.op_Implicit(Context.get_Request().get_Query().get_Item("data"));
			if (Received != null)
			{
				await Received(arg).PreserveCulture();
			}
		}

		private static Task WriteInit(LongPollingTransport transport)
		{
			transport.Context.get_Response().set_ContentType(transport.IsJsonp ? JsonUtility.JavaScriptMimeType : JsonUtility.JsonMimeType);
			return transport.Context.get_Response().Flush();
		}

		private static Task PerformKeepAlive(object state)
		{
			LongPollingTransport longPollingTransport = (LongPollingTransport)state;
			if (!longPollingTransport.IsAlive)
			{
				return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
			}
			longPollingTransport.Context.get_Response().Write(_keepAlive);
			return longPollingTransport.Context.get_Response().Flush();
		}

		private static Task PerformPartialSend(object state)
		{
			LongPollingTransportContext longPollingTransportContext = (LongPollingTransportContext)state;
			if (!longPollingTransportContext.Transport.IsAlive)
			{
				return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
			}
			using (BinaryMemoryPoolTextWriter binaryMemoryPoolTextWriter = new BinaryMemoryPoolTextWriter(longPollingTransportContext.Transport.Pool))
			{
				if (longPollingTransportContext.Transport.IsJsonp)
				{
					binaryMemoryPoolTextWriter.Write(longPollingTransportContext.Transport.JsonpCallback);
					binaryMemoryPoolTextWriter.Write("(");
				}
				longPollingTransportContext.Transport.JsonSerializer.Serialize(longPollingTransportContext.State, binaryMemoryPoolTextWriter);
				if (longPollingTransportContext.Transport.IsJsonp)
				{
					binaryMemoryPoolTextWriter.Write(");");
				}
				binaryMemoryPoolTextWriter.Flush();
				longPollingTransportContext.Transport.Context.get_Response().Write(binaryMemoryPoolTextWriter.Buffer);
			}
			return longPollingTransportContext.Transport.Context.get_Response().Flush();
		}

		private static Task PerformCompleteSend(object state)
		{
			LongPollingTransportContext longPollingTransportContext = (LongPollingTransportContext)state;
			if (!longPollingTransportContext.Transport.IsAlive)
			{
				return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
			}
			longPollingTransportContext.Transport.Context.get_Response().set_ContentType(longPollingTransportContext.Transport.IsJsonp ? JsonUtility.JavaScriptMimeType : JsonUtility.JsonMimeType);
			return PerformPartialSend(state);
		}

		private void AddTransportData(PersistentResponse response)
		{
			if (_pollDelay != TimeSpan.Zero)
			{
				response.LongPollDelay = (long)_pollDelay.TotalMilliseconds;
			}
		}
	}
}
