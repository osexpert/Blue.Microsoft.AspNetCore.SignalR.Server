using System;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class NotAuthorizedException : Exception
	{
		public NotAuthorizedException()
		{
		}

		public NotAuthorizedException(string message)
			: base(message)
		{
		}

		public NotAuthorizedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
