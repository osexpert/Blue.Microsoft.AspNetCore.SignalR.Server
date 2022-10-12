using System;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal static class MonoUtility
	{
		private static readonly Lazy<bool> _isRunningMono = new Lazy<bool>(() => CheckRunningOnMono());

		internal static bool IsRunningMono => _isRunningMono.Value;

		private static bool CheckRunningOnMono()
		{
			try
			{
				return (object)Type.GetType("Mono.Runtime") != null;
			}
			catch
			{
				return false;
			}
		}
	}
}
