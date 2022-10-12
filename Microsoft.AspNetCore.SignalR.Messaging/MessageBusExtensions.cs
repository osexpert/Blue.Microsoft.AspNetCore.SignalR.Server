using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Messaging
{
	public static class MessageBusExtensions
	{
		internal static Task Ack(this IMessageBus bus, string acker, string commandId)
		{
			Message message = new Message(acker, "__SIGNALR__SERVER__", (string)null);
			message.CommandId = commandId;
			message.IsAck = true;
			return bus.Publish(message);
		}
	}
}
