using Microsoft.AspNetCore.SignalR.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR
{
	public static class ConnectionExtensions
	{
		public static Task Send(this IConnection connection, string connectionId, object value)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			if (string.IsNullOrEmpty(connectionId))
			{
				throw new ArgumentException(Resources.Error_ArgumentNullOrEmpty, "connectionId");
			}
			ConnectionMessage message = new ConnectionMessage(PrefixHelper.GetConnectionId(connectionId), value);
			return connection.Send(message);
		}

		public static Task Send(this IConnection connection, IList<string> connectionIds, object value)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			if (connectionIds == null)
			{
				throw new ArgumentNullException("connectionIds");
			}
			ConnectionMessage message = new ConnectionMessage((from c in connectionIds
			select PrefixHelper.GetConnectionId(c)).ToList(), value);
			return connection.Send(message);
		}

		public static Task Broadcast(this IConnection connection, object value, params string[] excludeConnectionIds)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			ConnectionMessage message = new ConnectionMessage(connection.DefaultSignal, value, PrefixHelper.GetPrefixedConnectionIds(excludeConnectionIds));
			return connection.Send(message);
		}
	}
}
