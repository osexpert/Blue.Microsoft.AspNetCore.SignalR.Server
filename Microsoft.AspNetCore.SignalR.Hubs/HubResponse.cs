using Newtonsoft.Json;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class HubResponse
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
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

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public object Progress
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public bool? IsHubException
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string Error
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string StackTrace
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public object ErrorData
		{
			get;
			set;
		}
	}
}
