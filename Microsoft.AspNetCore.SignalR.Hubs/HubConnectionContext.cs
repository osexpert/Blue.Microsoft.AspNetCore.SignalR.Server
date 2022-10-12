using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class HubConnectionContext : HubConnectionContextBase, IHubCallerConnectionContext<object>, IHubConnectionContext<object>
	{
		private readonly string _connectionId;

		public dynamic Others
		{
			get;
			set;
		}

		public dynamic Caller
		{
			get;
			set;
		}

		public HubConnectionContext()
		{
			base.All = new NullClientProxy();
			Others = new NullClientProxy();
			Caller = new NullClientProxy();
		}

		public HubConnectionContext(IHubPipelineInvoker pipelineInvoker, IConnection connection, string hubName, string connectionId)
			: base(connection, pipelineInvoker, hubName)
		{
			_connectionId = connectionId;
			Caller = new SignalProxy(connection, pipelineInvoker, connectionId, hubName, "hc-", Microsoft.AspNetCore.SignalR.Infrastructure.ListHelper<string>.Empty);
			base.All = (object)AllExcept();
			Others = (object)AllExcept(connectionId);
		}

		public dynamic OthersInGroup(string groupName)
		{
			return Group(groupName, _connectionId);
		}

		public dynamic OthersInGroups(IList<string> groupNames)
		{
			return Groups(groupNames, _connectionId);
		}
	}
}
