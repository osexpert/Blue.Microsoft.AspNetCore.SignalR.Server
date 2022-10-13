using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	public class ServerSentEventsTransport : ForeverTransport
	{
		private class SendContext
		{
			public readonly ServerSentEventsTransport Transport;

			public readonly object State;

			public SendContext(ServerSentEventsTransport transport, object state)
			{
				Transport = transport;
				State = state;
			}
		}

		private static byte[] _keepAlive = Encoding.UTF8.GetBytes("data: {}\n\n");

		private static byte[] _dataInitialized = Encoding.UTF8.GetBytes("data: initialized\n\n");

		private readonly IPerformanceCounterManager _counters;

		public ServerSentEventsTransport(HttpContext context, JsonSerializer jsonSerializer, ITransportHeartbeat heartbeat, IPerformanceCounterManager performanceCounterManager, IApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory, IMemoryPool pool)
			: base(context, jsonSerializer, heartbeat, performanceCounterManager, applicationLifetime, loggerFactory, pool)
		{
			_counters = performanceCounterManager;
		}

		public override Task KeepAlive()
		{
			return EnqueueOperation((object state) => PerformKeepAlive(state), this);
		}

		public override Task Send(PersistentResponse response)
		{
			OnSendingResponse(response);
			SendContext state2 = new SendContext(this, response);
			return EnqueueOperation((object state) => PerformSend(state), state2);
		}

		public override void IncrementConnectionsCount()
		{
			_counters.ConnectionsCurrentServerSentEvents.Increment();
		}

		public override void DecrementConnectionsCount()
		{
			_counters.ConnectionsCurrentServerSentEvents.Decrement();
		}

		protected internal override Task InitializeResponse(ITransportConnection connection)
		{
			return base.InitializeResponse(connection).Then((ServerSentEventsTransport s) => WriteInit(s), this);
		}

		private static async Task PerformKeepAlive(object state)
		{
			ServerSentEventsTransport obj = (ServerSentEventsTransport)state;
			await obj.Context.Response.WriteAsync(new ArraySegment<byte>(_keepAlive));
			await  obj.Context.Response.Flush();
		}

		private static async Task PerformSend(object state)
		{
			SendContext sendContext = (SendContext)state;
			using (BinaryMemoryPoolTextWriter binaryMemoryPoolTextWriter = new BinaryMemoryPoolTextWriter(sendContext.Transport.Pool))
			{
				binaryMemoryPoolTextWriter.Write("data: ");
				sendContext.Transport.JsonSerializer.Serialize(sendContext.State, binaryMemoryPoolTextWriter);
				binaryMemoryPoolTextWriter.WriteLine();
				binaryMemoryPoolTextWriter.WriteLine();
				binaryMemoryPoolTextWriter.Flush();
				await sendContext.Transport.Context.Response.WriteAsync(binaryMemoryPoolTextWriter.Buffer);
			}
			await sendContext.Transport.Context.Response.Flush();
		}

		private static async Task WriteInit(ServerSentEventsTransport transport)
		{
		//	transport.Context.Features.Get<IHttpBufferingFeature>()?.DisableRequestBuffering();
			transport.Context.Response.ContentType = "text/event-stream";
			await transport.Context.Response.WriteAsync(new ArraySegment<byte>(_dataInitialized));
			await transport.Context.Response.Flush();
		}
	}
}
