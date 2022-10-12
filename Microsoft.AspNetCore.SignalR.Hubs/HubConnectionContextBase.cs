using Microsoft.AspNetCore.SignalR.Infrastructure;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class HubConnectionContextBase : IHubConnectionContext<object>
	{
		protected IHubPipelineInvoker Invoker
		{
			get;
			private set;
		}

		protected IConnection Connection
		{
			get;
			private set;
		}

		protected string HubName
		{
			get;
			private set;
		}

		public dynamic All
		{
			get;
			set;
		}

		public HubConnectionContextBase()
		{
		}

		public HubConnectionContextBase(IConnection connection, IHubPipelineInvoker invoker, string hubName)
		{
			Connection = connection;
			Invoker = invoker;
			HubName = hubName;
			All = (object)AllExcept();
		}

		public dynamic AllExcept(params string[] excludeConnectionIds)
		{
			return new ClientProxy(Connection, Invoker, HubName, PrefixHelper.GetPrefixedConnectionIds(excludeConnectionIds));
		}

		public dynamic Client(string connectionId)
		{
			if (string.IsNullOrEmpty(connectionId))
			{
				throw new ArgumentException(Resources.Error_ArgumentNullOrEmpty, "connectionId");
			}
			return new ConnectionIdProxy(Connection, Invoker, connectionId, HubName);
		}

		public dynamic Clients(IList<string> connectionIds)
		{
			if (connectionIds == null)
			{
				throw new ArgumentNullException("connectionIds");
			}
			return new MultipleSignalProxy(Connection, Invoker, connectionIds, HubName, "hc-", Microsoft.AspNetCore.SignalR.Infrastructure.ListHelper<string>.Empty);
		}

		public dynamic Group(string groupName, params string[] excludeConnectionIds)
		{
			if (string.IsNullOrEmpty(groupName))
			{
				throw new ArgumentException(Resources.Error_ArgumentNullOrEmpty, "groupName");
			}
			return new GroupProxy(Connection, Invoker, groupName, HubName, PrefixHelper.GetPrefixedConnectionIds(excludeConnectionIds));
		}

		public dynamic Groups(IList<string> groupNames, params string[] excludeConnectionIds)
		{
			if (groupNames == null)
			{
				throw new ArgumentNullException("groupNames");
			}
			return new MultipleSignalProxy(Connection, Invoker, groupNames, HubName, "hg-", PrefixHelper.GetPrefixedConnectionIds(excludeConnectionIds));
		}

		public dynamic User(string userId)
		{
			if (userId == null)
			{
				throw new ArgumentNullException("userId");
			}
			return new UserProxy(Connection, Invoker, userId, HubName);
		}

		public dynamic Users(IList<string> userIds)
		{
			if (userIds == null)
			{
				throw new ArgumentNullException("userIds");
			}
			return new MultipleSignalProxy(Connection, Invoker, userIds, HubName, "hu-", Microsoft.AspNetCore.SignalR.Infrastructure.ListHelper<string>.Empty);
		}
	}
}
