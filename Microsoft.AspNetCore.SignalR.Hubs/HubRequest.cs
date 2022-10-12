using Microsoft.AspNetCore.SignalR.Json;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class HubRequest
	{
		public string Hub
		{
			get;
			set;
		}

		public string Method
		{
			get;
			set;
		}

		public IJsonValue[] ParameterValues
		{
			get;
			set;
		}

		public string Id
		{
			get;
			set;
		}
	}
}
