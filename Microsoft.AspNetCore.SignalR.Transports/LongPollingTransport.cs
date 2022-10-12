using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

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

		private string JsonpCallback => base.Context.Request.Query["callback"];

		public override bool SupportsKeepAlive => !IsJsonp;

		public override bool RequiresTimeout => true;

		protected override int MaxMessages => 5000;

		protected override bool SuppressReconnect => !base.Context.Request.LocalPath().EndsWith("/reconnect", StringComparison.OrdinalIgnoreCase);

		public LongPollingTransport(HttpContext context, JsonSerializer jsonSerializer, ITransportHeartbeat heartbeat, IPerformanceCounterManager performanceCounterManager, IApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory, IOptions<SignalROptions> optionsAccessor, IMemoryPool pool)
			: base(context, jsonSerializer, heartbeat, performanceCounterManager, applicationLifetime, loggerFactory, pool)
		{
			_pollDelay = optionsAccessor.Value.Transports.LongPolling.PollDelay;
			_counters = performanceCounterManager;
		}

		protected override async Task InitializeMessageId()
		{
			_lastMessageId = Context.Request.Query["messageId"];
			if (_lastMessageId == null && Context.Request.HasFormContentType)
			{
				_lastMessageId = (await Context.Request.ReadFormAsync().PreserveCulture())["messageId"];
			}
		}

		public override async Task<string> GetGroupsToken()
		{
			StringValues values = Context.Request.Query["groupsToken"];
			if (values.Count == 0 && Context.Request.HasFormContentType)
			{
				values = (await Context.Request.ReadFormAsync().PreserveCulture())["groupsToken"];
			}
			return values;
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
			Task task = TaskAsyncHelper.Empty;
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
						return TaskAsyncHelper.False;
					});
				}
				return task.Then(delegate
				{
					_transportLifetime.Complete();
					return TaskAsyncHelper.False;
				});
			}
			_responseSent = true;
			return task.Then((LongPollingTransport transport, PersistentResponse resp) => transport.Send(resp), this, response).Then(() => TaskAsyncHelper.False);
		}

		protected internal override Task InitializeResponse(ITransportConnection connection)
		{
			return base.InitializeResponse(connection).Then((LongPollingTransport s) => WriteInit(s), this);
		}

		protected override async Task ProcessSendRequest()
		{
			if (string.IsNullOrEmpty(Context.Request.ContentType))
			{
				Context.Request.ContentType = ForeverTransport.FormContentType;
			}
			string arg = ((string)(await Context.Request.ReadFormAsync().PreserveCulture())["data"]) ?? ((string)Context.Request.Query["data"]);
			if (Received != null)
			{
				await Received(arg).PreserveCulture();
			}
		}

		private static Task WriteInit(LongPollingTransport transport)
		{
			transport.Context.Response.ContentType = (transport.IsJsonp ? JsonUtility.JavaScriptMimeType : JsonUtility.JsonMimeType);
			return transport.Context.Response.Flush();
		}

		private static async Task PerformKeepAlive(object state)
		{
			LongPollingTransport longPollingTransport = (LongPollingTransport)state;
			if (!longPollingTransport.IsAlive)
			{
				return;// TaskAsyncHelper.Empty;
			}
			await longPollingTransport.Context.Response.WriteAsync(_keepAlive);
			//return 
			await longPollingTransport.Context.Response.Flush();
		}

		private static async Task PerformPartialSend(object state)
		{
			LongPollingTransportContext longPollingTransportContext = (LongPollingTransportContext)state;
			if (!longPollingTransportContext.Transport.IsAlive)
			{
				return;// TaskAsyncHelper.Empty;
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
				await longPollingTransportContext.Transport.Context.Response.WriteAsync(binaryMemoryPoolTextWriter.Buffer);
			}
			//return 
			await longPollingTransportContext.Transport.Context.Response.Flush();
		}

		private static Task PerformCompleteSend(object state)
		{
			LongPollingTransportContext longPollingTransportContext = (LongPollingTransportContext)state;
			if (!longPollingTransportContext.Transport.IsAlive)
			{
				return TaskAsyncHelper.Empty;
			}
			longPollingTransportContext.Transport.Context.Response.ContentType = (longPollingTransportContext.Transport.IsJsonp ? JsonUtility.JavaScriptMimeType : JsonUtility.JsonMimeType);
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
