using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.WebSockets
{
	public abstract class WebSocketHandler
	{
		private class CloseContext
		{
			public WebSocketHandler Handler;

			public CloseContext(WebSocketHandler webSocketHandler)
			{
				Handler = webSocketHandler;
			}
		}

		private class SendContext
		{
			public WebSocketHandler Handler;

			public ArraySegment<byte> Message;

			public WebSocketMessageType MessageType;

			public bool EndOfMessage;

			public SendContext(WebSocketHandler webSocketHandler, ArraySegment<byte> message, WebSocketMessageType messageType, bool endOfMessage)
			{
				Handler = webSocketHandler;
				Message = message;
				MessageType = messageType;
				EndOfMessage = endOfMessage;
			}
		}

		private class ReceiveContext
		{
			public WebSocket WebSocket;

			public CancellationToken DisconnectToken;

			public int? MaxIncomingMessageSize;

			public int BufferSize;

			public ReceiveContext(WebSocket webSocket, CancellationToken disconnectToken, int? maxIncomingMessageSize, int bufferSize)
			{
				WebSocket = webSocket;
				DisconnectToken = disconnectToken;
				MaxIncomingMessageSize = maxIncomingMessageSize;
				BufferSize = bufferSize;
			}
		}

		private static readonly TimeSpan _closeTimeout = TimeSpan.FromMilliseconds(250.0);

		private const int _receiveLoopBufferSize = 4096;

		private readonly int? _maxIncomingMessageSize;

		private readonly ILogger _logger;

		private readonly Microsoft.AspNetCore.SignalR.Infrastructure.TaskQueue _sendQueue = new Microsoft.AspNetCore.SignalR.Infrastructure.TaskQueue();

		public int? MaxIncomingMessageSize => _maxIncomingMessageSize;

		internal WebSocket WebSocket
		{
			get;
			set;
		}

		public Exception Error
		{
			get;
			set;
		}

		protected WebSocketHandler(int? maxIncomingMessageSize, ILogger logger)
		{
			_maxIncomingMessageSize = maxIncomingMessageSize;
			_logger = logger;
		}

		public virtual void OnOpen()
		{
		}

		public virtual void OnMessage(string message)
		{
			throw new NotImplementedException();
		}

		public virtual void OnMessage(byte[] message)
		{
			throw new NotImplementedException();
		}

		public virtual void OnError()
		{
		}

		public virtual void OnClose()
		{
		}

		public Task SendAsync(string message)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(message);
			return SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text);
		}

		public virtual Task SendAsync(ArraySegment<byte> message, WebSocketMessageType messageType, bool endOfMessage = true)
		{
			if (GetWebSocketState(WebSocket) != WebSocketState.Open)
			{
				return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
			}
			SendContext state2 = new SendContext(this, message, messageType, endOfMessage);
			return _sendQueue.Enqueue(async delegate(object state)
			{
				SendContext sendContext = (SendContext)state;
				if (GetWebSocketState(sendContext.Handler.WebSocket) == WebSocketState.Open)
				{
					try
					{
						await sendContext.Handler.WebSocket.SendAsync(sendContext.Message, sendContext.MessageType, sendContext.EndOfMessage, CancellationToken.None).PreserveCulture();
					}
					catch (Exception arg)
					{
						LoggerExtensions.LogError(_logger, "Error while sending: " + arg, Array.Empty<object>());
					}
				}
			}, state2);
		}

		public virtual Task CloseAsync()
		{
			if (IsClosedOrClosedSent(WebSocket))
			{
				return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
			}
			CloseContext state2 = new CloseContext(this);
			return _sendQueue.Enqueue(async delegate(object state)
			{
				CloseContext closeContext = (CloseContext)state;
				if (!IsClosedOrClosedSent(closeContext.Handler.WebSocket))
				{
					try
					{
						await closeContext.Handler.WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).PreserveCulture();
					}
					catch (Exception arg)
					{
						LoggerExtensions.LogError(_logger, "Error while closing the websocket: " + arg, Array.Empty<object>());
					}
				}
			}, state2);
		}

		internal Task ProcessWebSocketRequestAsync(WebSocket webSocket, CancellationToken disconnectToken)
		{
			if (webSocket == null)
			{
				throw new ArgumentNullException("webSocket");
			}
			ReceiveContext state2 = new ReceiveContext(webSocket, disconnectToken, MaxIncomingMessageSize, 4096);
			return ProcessWebSocketRequestAsync(webSocket, disconnectToken, delegate(object state)
			{
				ReceiveContext receiveContext = (ReceiveContext)state;
				return WebSocketMessageReader.ReadMessageAsync(receiveContext.WebSocket, receiveContext.BufferSize, receiveContext.MaxIncomingMessageSize, receiveContext.DisconnectToken);
			}, state2);
		}

		internal async Task ProcessWebSocketRequestAsync(WebSocket webSocket, CancellationToken disconnectToken, Func<object, Task<WebSocketMessage>> messageRetriever, object state)
		{
			bool closedReceived = false;
			try
			{
				WebSocket = webSocket;
				OnOpen();
				while (!disconnectToken.IsCancellationRequested && !closedReceived)
				{
					WebSocketMessage webSocketMessage = await messageRetriever(state).PreserveCulture();
					switch (webSocketMessage.MessageType)
					{
					case WebSocketMessageType.Binary:
						OnMessage((byte[])webSocketMessage.Data);
						break;
					case WebSocketMessageType.Text:
						OnMessage((string)webSocketMessage.Data);
						break;
					default:
						closedReceived = true;
						await Task.WhenAny(CloseAsync(), Task.Delay(_closeTimeout)).PreserveCulture();
						break;
					}
				}
			}
			catch (OperationCanceledException error)
			{
				if (!disconnectToken.IsCancellationRequested)
				{
					Error = error;
					OnError();
				}
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex2)
			{
				if (IsFatalException(ex2))
				{
					Error = ex2;
					OnError();
				}
			}
			OnClose();
		}

		private static bool IsFatalException(Exception ex)
		{
			return true;
		}

		private static bool IsClosedOrClosedSent(WebSocket webSocket)
		{
			WebSocketState webSocketState = GetWebSocketState(webSocket);
			if (webSocketState != WebSocketState.Closed && webSocketState != WebSocketState.CloseSent)
			{
				return webSocketState == WebSocketState.Aborted;
			}
			return true;
		}

		private static WebSocketState GetWebSocketState(WebSocket webSocket)
		{
			try
			{
				return webSocket.State;
			}
			catch (ObjectDisposedException)
			{
				return WebSocketState.Closed;
			}
		}
	}
}
