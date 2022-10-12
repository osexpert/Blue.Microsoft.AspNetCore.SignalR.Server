using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

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

		private static Task PerformKeepAlive(object state)
		{
			ServerSentEventsTransport obj = (ServerSentEventsTransport)state;
			obj.Context.get_Response().Write(new ArraySegment<byte>(_keepAlive));
			return obj.Context.get_Response().Flush();
		}

		private static Task PerformSend(object state)
		{
			SendContext sendContext = (SendContext)state;
			using (BinaryMemoryPoolTextWriter binaryMemoryPoolTextWriter = new BinaryMemoryPoolTextWriter(sendContext.Transport.Pool))
			{
				binaryMemoryPoolTextWriter.Write("data: ");
				sendContext.Transport.JsonSerializer.Serialize(sendContext.State, binaryMemoryPoolTextWriter);
				binaryMemoryPoolTextWriter.WriteLine();
				binaryMemoryPoolTextWriter.WriteLine();
				binaryMemoryPoolTextWriter.Flush();
				sendContext.Transport.Context.get_Response().Write(binaryMemoryPoolTextWriter.Buffer);
			}
			return sendContext.Transport.Context.get_Response().Flush();
		}

		private static Task WriteInit(ServerSentEventsTransport transport)
		{
			IHttpBufferingFeature val = transport.Context.get_Features().Get<IHttpBufferingFeature>();
			if (val != null)
			{
				val.DisableRequestBuffering();
			}
			transport.Context.get_Response().set_ContentType("text/event-stream");
			transport.Context.get_Response().Write(new ArraySegment<byte>(_dataInitialized));
			return transport.Context.get_Response().Flush();
		}
	}
}
