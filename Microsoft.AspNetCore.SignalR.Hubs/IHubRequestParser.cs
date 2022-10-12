using Newtonsoft.Json;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IHubRequestParser
	{
		HubRequest Parse(string data, JsonSerializer serializer);
	}
}
