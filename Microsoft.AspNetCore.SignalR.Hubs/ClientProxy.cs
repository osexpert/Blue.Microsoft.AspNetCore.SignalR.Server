using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class ClientProxy : DynamicObject, IClientProxy
	{
		private readonly IHubPipelineInvoker _invoker;

		private readonly IConnection _connection;

		private readonly string _hubName;

		private readonly string _signal;

		private readonly IList<string> _exclude;

		public ClientProxy(IConnection connection, IHubPipelineInvoker invoker, string hubName, IList<string> exclude)
		{
			_connection = connection;
			_invoker = invoker;
			_hubName = hubName;
			_exclude = exclude;
			_signal = PrefixHelper.GetHubName(_hubName);
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			result = Invoke(binder.Name, args);
			return true;
		}

		public Task Invoke(string method, params object[] args)
		{
			ClientHubInvocation invocation = new ClientHubInvocation
			{
				Hub = _hubName,
				Method = method,
				Args = args
			};
			HubOutgoingInvokerContext context = new HubOutgoingInvokerContext(_connection, _signal, invocation)
			{
				ExcludedSignals = _exclude
			};
			return _invoker.Send(context);
		}
	}
}
