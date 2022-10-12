using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Json;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IHubManager
	{
		HubDescriptor GetHub(string hubName);

		IEnumerable<HubDescriptor> GetHubs(Func<HubDescriptor, bool> predicate);

		IHub ResolveHub(string hubName);

		IEnumerable<IHub> ResolveHubs();

		MethodDescriptor GetHubMethod(string hubName, string method, IList<IJsonValue> parameters);

		IEnumerable<MethodDescriptor> GetHubMethods(string hubName, Func<MethodDescriptor, bool> predicate);
	}
}
