using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Json;
using Microsoft.AspNetCore.SignalR.WebSockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	public class WebSocketTransport : ForeverTransport
	{
		private class WebSocketTransportContext
		{
			public readonly WebSocketTransport Transport;

			public readonly object State;

			public WebSocketTransportContext(WebSocketTransport transport, object state)
			{
				Transport = transport;
				State = state;
			}
		}

		private readonly HttpContext _context;

		private IWebSocket _socket;

		private bool _isAlive = true;

		private readonly int? _maxIncomingMessageSize;

		private readonly Action<string> _message;

		private readonly Action _closed;

		private readonly Action<Exception> _error;

		private readonly IPerformanceCounterManager _counters;

		private static byte[] _keepAlive = Encoding.UTF8.GetBytes("{}");

		public override bool IsAlive => _isAlive;

		public override CancellationToken CancellationToken => CancellationToken.None;

		public WebSocketTransport(HttpContext context, JsonSerializer serializer, ITransportHeartbeat heartbeat, IPerformanceCounterManager performanceCounterManager, IApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory, IMemoryPool pool)
			: this(context, serializer, heartbeat, performanceCounterManager, applicationLifetime, loggerFactory, pool, null)
		{
		}

		public WebSocketTransport(HttpContext context, JsonSerializer serializer, ITransportHeartbeat heartbeat, IPerformanceCounterManager performanceCounterManager, IApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory, IMemoryPool pool, int? maxIncomingMessageSize)
			: base(context, serializer, heartbeat, performanceCounterManager, applicationLifetime, loggerFactory, pool)
		{
			_context = context;
			_maxIncomingMessageSize = maxIncomingMessageSize;
			_message = OnMessage;
			_closed = OnClosed;
			_error = OnSocketError;
			_counters = performanceCounterManager;
		}

		public override Task KeepAlive()
		{
			return EnqueueOperation((object state) => ((IWebSocket)state).Send(new ArraySegment<byte>(_keepAlive)), _socket);
		}

		public override Task ProcessRequest(ITransportConnection connection)
		{
			if (base.IsAbortRequest)
			{
				return connection.Abort(base.ConnectionId);
			}
			return AcceptWebSocketRequest(connection);
		}

		public override Task Send(object value)
		{
			WebSocketTransportContext state2 = new WebSocketTransportContext(this, value);
			return EnqueueOperation((object state) => PerformSend(state), state2);
		}

		public override Task Send(PersistentResponse response)
		{
			OnSendingResponse(response);
			return Send((object)response);
		}

		public override void IncrementConnectionsCount()
		{
			_counters.ConnectionsCurrentWebSockets.Increment();
		}

		public override void DecrementConnectionsCount()
		{
			_counters.ConnectionsCurrentWebSockets.Decrement();
		}

		private async Task AcceptWebSocketRequest(ITransportConnection connection)
		{
			DefaultWebSocketHandler handler = (DefaultWebSocketHandler)(_socket = new DefaultWebSocketHandler(_maxIncomingMessageSize, Logger));
			_socket.OnClose = _closed;
			_socket.OnMessage = _message;
			_socket.OnError = _error;
			WebSocket webSocket;
			try
			{
				webSocket = await Context.get_WebSockets().AcceptWebSocketAsync();
			}
			catch
			{
				_context.get_Response().set_StatusCode(400);
				await HttpResponseWritingExtensions.WriteAsync(_context.get_Response(), Resources.Error_NotWebSocketRequest, default(CancellationToken));
				return;
			}
			Task task = handler.ProcessWebSocketRequestAsync(webSocket, CancellationToken);
			ProcessRequestCore(connection).ContinueWith((Func<Task, object, Task>)async delegate(Task _, object state)
			{
				await((DefaultWebSocketHandler)state).CloseAsync();
			}, (object)handler);
			await task;
		}

		private static async Task PerformSend(object state)
		{
			WebSocketTransportContext context = (WebSocketTransportContext)state;
			IWebSocket socket = context.Transport._socket;
			using (BinaryMemoryPoolTextWriter writer = new BinaryMemoryPoolTextWriter(context.Transport.Pool))
			{
				try
				{
					context.Transport.JsonSerializer.Serialize(context.State, writer);
					writer.Flush();
					await socket.Send(writer.Buffer).PreserveCulture();
				}
				catch (Exception ex)
				{
					context.Transport.OnError(ex);
					throw;
				}
			}
		}

		private void OnMessage(string message)
		{
			if (base.Received != null)
			{
				base.Received(message).Catch(base.Logger);
			}
		}

		private void OnClosed()
		{
			LoggerExtensions.LogInformation(base.Logger, $"CloseSocket({base.ConnectionId})", Array.Empty<object>());
			_isAlive = false;
		}

		private void OnSocketError(Exception error)
		{
			LoggerExtensions.LogError(base.Logger, $"OnError({base.ConnectionId}, {error})", Array.Empty<object>());
		}
	}
}
