using Microsoft.AspNetCore.SignalR.Json;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IMethodDescriptorProvider
	{
		IEnumerable<MethodDescriptor> GetMethods(HubDescriptor hub);

		bool TryGetMethod(HubDescriptor hub, string method, out MethodDescriptor descriptor, IList<IJsonValue> parameters);
	}
}
