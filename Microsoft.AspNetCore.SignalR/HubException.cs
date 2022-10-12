using System;

namespace Microsoft.AspNetCore.SignalR
{
	public class HubException : Exception
	{
		public object ErrorData
		{
			get;
			private set;
		}

		public HubException()
		{
		}

		public HubException(string message)
			: base(message)
		{
		}

		public HubException(string message, object errorData)
			: base(message)
		{
			ErrorData = errorData;
		}
	}
}
