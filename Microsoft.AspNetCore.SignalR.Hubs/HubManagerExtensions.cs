using Microsoft.AspNetCore.SignalR.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public static class HubManagerExtensions
	{
		public static HubDescriptor EnsureHub(this IHubManager hubManager, string hubName, params IPerformanceCounter[] counters)
		{
			if (hubManager == null)
			{
				throw new ArgumentNullException("hubManager");
			}
			if (string.IsNullOrEmpty(hubName))
			{
				throw new ArgumentNullException("hubName");
			}
			if (counters == null)
			{
				throw new ArgumentNullException("counters");
			}
			HubDescriptor hub = hubManager.GetHub(hubName);
			if (hub == null)
			{
				for (int i = 0; i < counters.Length; i++)
				{
					counters[i].Increment();
				}
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_HubCouldNotBeResolved, hubName));
			}
			return hub;
		}

		public static IEnumerable<HubDescriptor> GetHubs(this IHubManager hubManager)
		{
			if (hubManager == null)
			{
				throw new ArgumentNullException("hubManager");
			}
			return hubManager.GetHubs((HubDescriptor d) => true);
		}

		public static IEnumerable<MethodDescriptor> GetHubMethods(this IHubManager hubManager, string hubName)
		{
			if (hubManager == null)
			{
				throw new ArgumentNullException("hubManager");
			}
			return hubManager.GetHubMethods(hubName, (MethodDescriptor m) => true);
		}
	}
}
