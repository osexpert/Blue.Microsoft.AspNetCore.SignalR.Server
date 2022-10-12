using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Messaging;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	internal static class TransportConnectionExtensions
	{
		internal static Task Initialize(this ITransportConnection connection, string connectionId)
		{
			return SendCommand(connection, connectionId, CommandType.Initializing);
		}

		internal static Task Abort(this ITransportConnection connection, string connectionId)
		{
			return SendCommand(connection, connectionId, CommandType.Abort);
		}

		private static Task SendCommand(ITransportConnection connection, string connectionId, CommandType commandType)
		{
			Command value = new Command
			{
				CommandType = commandType
			};
			ConnectionMessage message = new ConnectionMessage(PrefixHelper.GetConnectionId(connectionId), value);
			return connection.Send(message);
		}
	}
}
