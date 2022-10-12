using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class MultipleSignalProxy : DynamicObject, IClientProxy
	{
		private readonly IConnection _connection;

		private readonly IHubPipelineInvoker _invoker;

		private readonly IList<string> _exclude;

		private readonly IList<string> _signals;

		private readonly string _hubName;

		public MultipleSignalProxy(IConnection connection, IHubPipelineInvoker invoker, IList<string> signals, string hubName, string prefix, IList<string> exclude)
		{
			MultipleSignalProxy multipleSignalProxy = this;
			_connection = connection;
			_invoker = invoker;
			_hubName = hubName;
			_signals = signals.Select((string signal) => prefix + multipleSignalProxy._hubName + "." + signal).ToList();
			_exclude = exclude;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = null;
			return false;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			result = Invoke(binder.Name, args);
			return true;
		}

		public Task Invoke(string method, params object[] args)
		{
			ClientHubInvocation invocationData = GetInvocationData(method, args);
			HubOutgoingInvokerContext context = new HubOutgoingInvokerContext(_connection, _signals, invocationData)
			{
				ExcludedSignals = _exclude
			};
			return _invoker.Send(context);
		}

		protected virtual ClientHubInvocation GetInvocationData(string method, object[] args)
		{
			return new ClientHubInvocation
			{
				Hub = _hubName,
				Method = method,
				Args = args
			};
		}
	}
}
