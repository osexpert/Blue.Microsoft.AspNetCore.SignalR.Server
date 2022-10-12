using Newtonsoft.Json;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class HubResponse
	{
		[JsonProperty("R", NullValueHandling = NullValueHandling.Ignore)]
		public object Result
		{
			get;
			set;
		}

		[JsonProperty("I")]
		public string Id
		{
			get;
			set;
		}

		[JsonProperty("P", NullValueHandling = NullValueHandling.Ignore)]
		public object Progress
		{
			get;
			set;
		}

		[JsonProperty("H", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsHubException
		{
			get;
			set;
		}

		[JsonProperty("E", NullValueHandling = NullValueHandling.Ignore)]
		public string Error
		{
			get;
			set;
		}

		[JsonProperty("T", NullValueHandling = NullValueHandling.Ignore)]
		public string StackTrace
		{
			get;
			set;
		}

		[JsonProperty("D", NullValueHandling = NullValueHandling.Ignore)]
		public object ErrorData
		{
			get;
			set;
		}
	}
}
