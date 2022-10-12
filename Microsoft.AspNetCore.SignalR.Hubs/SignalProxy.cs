using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class SignalProxy : DynamicObject, IClientProxy
	{
		private readonly IList<string> _exclude;

		protected IConnection Connection
		{
			get;
			private set;
		}

		protected IHubPipelineInvoker Invoker
		{
			get;
			private set;
		}

		protected string Signal
		{
			get;
			private set;
		}

		protected string HubName
		{
			get;
			private set;
		}

		public SignalProxy(IConnection connection, IHubPipelineInvoker invoker, string signal, string hubName, string prefix, IList<string> exclude)
		{
			Connection = connection;
			Invoker = invoker;
			HubName = hubName;
			Signal = prefix + hubName + "." + signal;
			_exclude = exclude;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			result = Invoke(binder.Name, args);
			return true;
		}

		public Task Invoke(string method, params object[] args)
		{
			ClientHubInvocation invocationData = GetInvocationData(method, args);
			HubOutgoingInvokerContext context = new HubOutgoingInvokerContext(Connection, Signal, invocationData)
			{
				ExcludedSignals = _exclude
			};
			return Invoker.Send(context);
		}

		protected virtual ClientHubInvocation GetInvocationData(string method, object[] args)
		{
			return new ClientHubInvocation
			{
				Hub = HubName,
				Method = method,
				Args = args
			};
		}
	}
}
