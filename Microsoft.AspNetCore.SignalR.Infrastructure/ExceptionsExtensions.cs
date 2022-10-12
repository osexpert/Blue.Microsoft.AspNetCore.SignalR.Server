using System;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal static class ExceptionsExtensions
	{
		internal static Exception Unwrap(this Exception ex)
		{
			if (ex == null)
			{
				return null;
			}
			Exception ex2 = ex.GetBaseException();
			while (ex2.InnerException != null)
			{
				ex2 = ex2.InnerException;
			}
			return ex2;
		}
	}
}
