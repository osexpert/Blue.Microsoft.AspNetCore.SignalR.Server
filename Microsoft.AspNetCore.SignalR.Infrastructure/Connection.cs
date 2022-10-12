using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Json;
using Microsoft.AspNetCore.SignalR.Messaging;
using Microsoft.AspNetCore.SignalR.Transports;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public class Connection : IConnection, ITransportConnection, ISubscriber
	{
		private class ReceiveContext
		{
			private readonly Connection _connection;

			private readonly Func<PersistentResponse, object, Task<bool>> _callback;

			private readonly object _callbackState;

			public ReceiveContext(Connection connection, Func<PersistentResponse, object, Task<bool>> callback, object callbackState)
			{
				_connection = connection;
				_callback = callback;
				_callbackState = callbackState;
			}

			public Task<bool> InvokeCallback(MessageResult result)
			{
				PersistentResponse response = _connection.GetResponse(result);
				return _callback(response, _callbackState);
			}
		}

		private readonly IMessageBus _bus;

		private readonly JsonSerializer _serializer;

		private readonly string _baseSignal;

		private readonly string _connectionId;

		private readonly IList<string> _signals;

		private readonly DiffSet<string> _groups;

		private readonly IPerformanceCounterManager _counters;

		private bool _aborted;

		private bool _initializing;

		private readonly ILogger _logger;

		private readonly IAckHandler _ackHandler;

		private readonly IProtectedData _protectedData;

		private readonly Func<Message, bool> _excludeMessage;

		private readonly IMemoryPool _pool;

		public string DefaultSignal => _baseSignal;

		IList<string> ISubscriber.EventKeys => _signals;

		public Action<TextWriter> WriteCursor
		{
			get;
			set;
		}

		public string Identity => _connectionId;

		public Subscription Subscription
		{
			get;
			set;
		}

		public event Action<ISubscriber, string> EventKeyAdded;

		public event Action<ISubscriber, string> EventKeyRemoved;

		public Connection(IMessageBus newMessageBus, JsonSerializer jsonSerializer, string baseSignal, string connectionId, IList<string> signals, IList<string> groups, ILoggerFactory loggerFactory, IAckHandler ackHandler, IPerformanceCounterManager performanceCounterManager, IProtectedData protectedData, IMemoryPool pool)
		{
			if (loggerFactory == null)
			{
				throw new ArgumentNullException("loggerFactory");
			}
			_bus = newMessageBus;
			_serializer = jsonSerializer;
			_baseSignal = baseSignal;
			_connectionId = connectionId;
			_signals = new List<string>(signals.Concat(groups));
			_groups = new DiffSet<string>(groups);
			_logger = loggerFactory.CreateLogger<Connection>();
			_ackHandler = ackHandler;
			_counters = performanceCounterManager;
			_protectedData = protectedData;
			_excludeMessage = (Message m) => ExcludeMessage(m);
			_pool = pool;
		}

		public Task Send(ConnectionMessage message)
		{
			if (!string.IsNullOrEmpty(message.Signal) && message.Signals != null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_AmbiguousMessage, message.Signal, string.Join(", ", message.Signals)));
			}
			if (message.Signals != null)
			{
				return MultiSend(message.Signals, message.Value, message.ExcludedSignals);
			}
			Message message2 = CreateMessage(message.Signal, message.Value);
			message2.Filter = GetFilter(message.ExcludedSignals);
			if (message2.WaitForAck)
			{
				Task arg = _ackHandler.CreateAck(message2.CommandId);
				return _bus.Publish(message2).Then((Task task) => task, arg);
			}
			return _bus.Publish(message2);
		}

		private Task MultiSend(IList<string> signals, object value, IList<string> excludedSignals)
		{
			if (signals.Count == 0)
			{
				return TaskAsyncHelper.Empty;
			}
			ArraySegment<byte> messageBuffer = GetMessageBuffer(value);
			string filter = GetFilter(excludedSignals);
			Task[] array = new Task[signals.Count];
			for (int i = 0; i < signals.Count; i++)
			{
				Message message = new Message(_connectionId, signals[i], messageBuffer);
				if (!string.IsNullOrEmpty(filter))
				{
					message.Filter = filter;
				}
				array[i] = _bus.Publish(message);
			}
			return Task.WhenAll(array);
		}

		private static string GetFilter(IList<string> excludedSignals)
		{
			if (excludedSignals != null)
			{
				return string.Join("|", excludedSignals);
			}
			return null;
		}

		private Message CreateMessage(string key, object value)
		{
			ArraySegment<byte> messageBuffer = GetMessageBuffer(value);
			Message message = new Message(_connectionId, key, messageBuffer);
			Command command = value as Command;
			if (command != null)
			{
				message.CommandId = command.Id;
				message.WaitForAck = command.WaitForAck;
			}
			return message;
		}

		private ArraySegment<byte> GetMessageBuffer(object value)
		{
			if (value is ArraySegment<byte>)
			{
				return (ArraySegment<byte>)value;
			}
			return SerializeMessageValue(value);
		}

		private ArraySegment<byte> SerializeMessageValue(object value)
		{
			using (MemoryPoolTextWriter memoryPoolTextWriter = new MemoryPoolTextWriter(_pool))
			{
				_serializer.Serialize(value, memoryPoolTextWriter);
				memoryPoolTextWriter.Flush();
				ArraySegment<byte> buffer = memoryPoolTextWriter.Buffer;
				byte[] array = new byte[buffer.Count];
				Buffer.BlockCopy(buffer.Array, buffer.Offset, array, 0, buffer.Count);
				return new ArraySegment<byte>(array);
			}
		}

		public IDisposable Receive(string messageId, Func<PersistentResponse, object, Task<bool>> callback, int maxMessages, object state)
		{
			ReceiveContext state2 = new ReceiveContext(this, callback, state);
			return _bus.Subscribe(this, messageId, (MessageResult result, object s) => MessageBusCallback(result, s), maxMessages, state2);
		}

		private static Task<bool> MessageBusCallback(MessageResult result, object state)
		{
			return ((ReceiveContext)state).InvokeCallback(result);
		}

		private PersistentResponse GetResponse(MessageResult result)
		{
			ProcessResults(result);
			PersistentResponse persistentResponse = new PersistentResponse(_excludeMessage, WriteCursor);
			persistentResponse.Terminal = result.Terminal;
			if (!result.Terminal)
			{
				persistentResponse.Messages = result.Messages;
				persistentResponse.Aborted = _aborted;
				persistentResponse.TotalCount = result.TotalCount;
				persistentResponse.Initializing = _initializing;
				_initializing = false;
			}
			PopulateResponseState(persistentResponse);
			_counters.ConnectionMessagesReceivedTotal.IncrementBy(result.TotalCount);
			_counters.ConnectionMessagesReceivedPerSec.IncrementBy(result.TotalCount);
			return persistentResponse;
		}

		private bool ExcludeMessage(Message message)
		{
			if (string.IsNullOrEmpty(message.Filter))
			{
				return false;
			}
			return message.Filter.Split(new char[1]
			{
				'|'
			}).Any((string signal) => Identity.Equals(signal, StringComparison.OrdinalIgnoreCase) || _signals.Contains(signal) || _groups.Contains(signal));
		}

		private void ProcessResults(MessageResult result)
		{
			result.Messages.Enumerate((Message message) => message.IsCommand, delegate(Connection connection, Message message)
			{
				ProcessResultsCore(connection, message);
			}, this);
		}

		private static void ProcessResultsCore(Connection connection, Message message)
		{
			if (message.IsAck)
			{
				connection._logger.LogError("Connection {0} received an unexpected ACK message.", connection.Identity);
				return;
			}
			Command command = connection._serializer.Parse<Command>(message.Value, message.Encoding);
			connection.ProcessCommand(command);
			if (message.WaitForAck)
			{
				connection._bus.Ack(connection._connectionId, message.CommandId).Catch(connection._logger);
			}
		}

		private void ProcessCommand(Command command)
		{
			switch (command.CommandType)
			{
			case CommandType.AddToGroup:
			{
				string value2 = command.Value;
				if (this.EventKeyAdded != null)
				{
					_groups.Add(value2);
					this.EventKeyAdded(this, value2);
				}
				break;
			}
			case CommandType.RemoveFromGroup:
			{
				string value = command.Value;
				if (this.EventKeyRemoved != null)
				{
					_groups.Remove(value);
					this.EventKeyRemoved(this, value);
				}
				break;
			}
			case CommandType.Initializing:
				_initializing = true;
				break;
			case CommandType.Abort:
				_aborted = true;
				break;
			}
		}

		private void PopulateResponseState(PersistentResponse response)
		{
			PopulateResponseState(response, _groups, _serializer, _protectedData, _connectionId);
		}

		internal static void PopulateResponseState(PersistentResponse response, DiffSet<string> groupSet, JsonSerializer serializer, IProtectedData protectedData, string connectionId)
		{
			if (groupSet.DetectChanges())
			{
				IEnumerable<string> snapshot = groupSet.GetSnapshot();
				string data = connectionId + ":" + serializer.Stringify(PrefixHelper.RemoveGroupPrefixes(snapshot));
				response.GroupsToken = protectedData.Protect(data, "SignalR.Groups.v1.1");
			}
		}
	}
}
