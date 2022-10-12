using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IHubDescriptorProvider
	{
		IList<HubDescriptor> GetHubs();

		bool TryGetHub(string hubName, out HubDescriptor descriptor);
	}
}
