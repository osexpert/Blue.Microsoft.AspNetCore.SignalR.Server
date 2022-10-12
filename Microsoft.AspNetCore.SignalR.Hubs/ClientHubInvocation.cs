using Newtonsoft.Json;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class ClientHubInvocation
	{
		[JsonProperty("H")]
		public string Hub
		{
			get;
			set;
		}

		[JsonProperty("M")]
		public string Method
		{
			get;
			set;
		}

		[JsonProperty("A")]
		public object[] Args
		{
			get;
			set;
		}
	}
}
