using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Messaging;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal class AckSubscriber : ISubscriber, IDisposable
	{
		private readonly IMessageBus _messageBus;

		private readonly IAckHandler _ackHandler;

		private IDisposable _subscription;

		private const int MaxMessages = 10;

		private static readonly string[] ServerSignals = new string[1]
		{
			"__SIGNALR__SERVER__"
		};

		public const string Signal = "__SIGNALR__SERVER__";

		public IList<string> EventKeys => ServerSignals;

		public Action<TextWriter> WriteCursor
		{
			get;
			set;
		}

		public string Identity
		{
			get;
			private set;
		}

		public Subscription Subscription
		{
			get;
			set;
		}

		public event Action<ISubscriber, string> EventKeyAdded
		{
			add
			{
			}
			remove
			{
			}
		}

		public event Action<ISubscriber, string> EventKeyRemoved
		{
			add
			{
			}
			remove
			{
			}
		}

		public AckSubscriber(IMessageBus messageBus, IAckHandler ackHandler)
		{
			_messageBus = messageBus;
			_ackHandler = ackHandler;
			Identity = Guid.NewGuid().ToString();
			ProcessMessages();
		}

		public void Dispose()
		{
			if (_subscription != null)
			{
				_subscription.Dispose();
			}
		}

		private void ProcessMessages()
		{
			_subscription = _messageBus.Subscribe(this, null, TriggerAcks, 10, null);
		}

		private Task<bool> TriggerAcks(MessageResult result, object state)
		{
			result.Messages.Enumerate((Message m) => m.IsAck, delegate(object s, Message m)
			{
				((IAckHandler)s).TriggerAck(m.CommandId);
			}, _ackHandler);
			return TaskAsyncHelper.True;
		}
	}
}
