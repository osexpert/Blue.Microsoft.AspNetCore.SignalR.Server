using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR
{
	public class GroupManager : IConnectionGroupManager, IGroupManager
	{
		private readonly IConnection _connection;

		private readonly string _groupPrefix;

		public GroupManager(IConnection connection, string groupPrefix)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			_connection = connection;
			_groupPrefix = groupPrefix;
		}

		public Task Send(string groupName, object value, params string[] excludeConnectionIds)
		{
			if (string.IsNullOrEmpty(groupName))
			{
				throw new ArgumentException(Resources.Error_ArgumentNullOrEmpty, "groupName");
			}
			string signal = CreateQualifiedName(groupName);
			ConnectionMessage message = new ConnectionMessage(signal, value, PrefixHelper.GetPrefixedConnectionIds(excludeConnectionIds));
			return _connection.Send(message);
		}

		public Task Send(IList<string> groupNames, object value, params string[] excludeConnectionIds)
		{
			if (groupNames == null)
			{
				throw new ArgumentNullException("groupNames");
			}
			ConnectionMessage message = new ConnectionMessage((from groupName in groupNames
			select CreateQualifiedName(groupName)).ToList(), value, PrefixHelper.GetPrefixedConnectionIds(excludeConnectionIds));
			return _connection.Send(message);
		}

		public Task Add(string connectionId, string groupName)
		{
			if (connectionId == null)
			{
				throw new ArgumentNullException("connectionId");
			}
			if (groupName == null)
			{
				throw new ArgumentNullException("groupName");
			}
			Command value = new Command
			{
				CommandType = CommandType.AddToGroup,
				Value = CreateQualifiedName(groupName),
				WaitForAck = true
			};
			return _connection.Send(connectionId, value);
		}

		public Task Remove(string connectionId, string groupName)
		{
			if (connectionId == null)
			{
				throw new ArgumentNullException("connectionId");
			}
			if (groupName == null)
			{
				throw new ArgumentNullException("groupName");
			}
			Command value = new Command
			{
				CommandType = CommandType.RemoveFromGroup,
				Value = CreateQualifiedName(groupName),
				WaitForAck = true
			};
			return _connection.Send(connectionId, value);
		}

		private string CreateQualifiedName(string groupName)
		{
			return _groupPrefix + "." + groupName;
		}
	}
}
