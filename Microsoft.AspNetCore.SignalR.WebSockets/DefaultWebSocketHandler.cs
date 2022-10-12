using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.WebSockets
{
	public class DefaultWebSocketHandler : WebSocketHandler, IWebSocket
	{
		private readonly IWebSocket _webSocket;

		private volatile bool _closed;

		Action<string> IWebSocket.OnMessage
		{
			get;
			set;
		}

		Action IWebSocket.OnClose
		{
			get;
			set;
		}

		Action<Exception> IWebSocket.OnError
		{
			get;
			set;
		}

		public DefaultWebSocketHandler(int? maxIncomingMessageSize, ILogger logger)
			: base(maxIncomingMessageSize, logger)
		{
			_webSocket = this;
			_webSocket.OnClose = delegate
			{
			};
			_webSocket.OnError = delegate
			{
			};
			_webSocket.OnMessage = delegate
			{
			};
		}

		public override void OnClose()
		{
			_closed = true;
			_webSocket.OnClose();
		}

		public override void OnError()
		{
			_webSocket.OnError(base.Error);
		}

		public override void OnMessage(string message)
		{
			_webSocket.OnMessage(message);
		}

		public Task Send(ArraySegment<byte> message)
		{
			if (_closed)
			{
				return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
			}
			return base.SendAsync(message, WebSocketMessageType.Text);
		}

		public override Task CloseAsync()
		{
			if (_closed)
			{
				return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
			}
			return base.CloseAsync();
		}
	}
}
