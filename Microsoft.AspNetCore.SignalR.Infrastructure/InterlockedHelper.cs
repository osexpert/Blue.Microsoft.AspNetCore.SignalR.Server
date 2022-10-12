using System.Threading;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public static class InterlockedHelper
	{
		public static bool CompareExchangeOr(ref int location, int value, int comparandA, int comparandB)
		{
			if (Interlocked.CompareExchange(ref location, value, comparandA) != comparandA)
			{
				return Interlocked.CompareExchange(ref location, value, comparandB) == comparandB;
			}
			return true;
		}
	}
}
