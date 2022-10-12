using Microsoft.AspNetCore.SignalR.Infrastructure;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR
{
	public struct ConnectionMessage
	{
		public string Signal
		{
			get;
			private set;
		}

		public IList<string> Signals
		{
			get;
			private set;
		}

		public object Value
		{
			get;
			private set;
		}

		public IList<string> ExcludedSignals
		{
			get;
			private set;
		}

		public ConnectionMessage(IList<string> signals, object value)
		{
			this = new ConnectionMessage(signals, value, Microsoft.AspNetCore.SignalR.Infrastructure.ListHelper<string>.Empty);
		}

		public ConnectionMessage(IList<string> signals, object value, IList<string> excludedSignals)
		{
			this = default(ConnectionMessage);
			Signals = signals;
			Value = value;
			ExcludedSignals = excludedSignals;
		}

		public ConnectionMessage(string signal, object value)
		{
			this = new ConnectionMessage(signal, value, Microsoft.AspNetCore.SignalR.Infrastructure.ListHelper<string>.Empty);
		}

		public ConnectionMessage(string signal, object value, IList<string> excludedSignals)
		{
			this = default(ConnectionMessage);
			Signal = signal;
			Value = value;
			ExcludedSignals = excludedSignals;
		}
	}
}
