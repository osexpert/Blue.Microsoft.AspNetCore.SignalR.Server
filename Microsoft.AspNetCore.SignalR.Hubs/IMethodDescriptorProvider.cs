using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Json;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IMethodDescriptorProvider
	{
		IEnumerable<MethodDescriptor> GetMethods(HubDescriptor hub);

		bool TryGetMethod(HubDescriptor hub, string method, out MethodDescriptor descriptor, IList<IJsonValue> parameters);
	}
}
