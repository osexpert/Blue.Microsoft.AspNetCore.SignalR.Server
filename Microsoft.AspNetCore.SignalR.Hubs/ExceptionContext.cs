using System;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class ExceptionContext
	{
		private object _result;

		public Exception Error
		{
			get;
			set;
		}

		public object Result
		{
			get
			{
				return _result;
			}
			set
			{
				Error = null;
				_result = value;
			}
		}

		public ExceptionContext(Exception error)
		{
			Error = error;
		}
	}
}
