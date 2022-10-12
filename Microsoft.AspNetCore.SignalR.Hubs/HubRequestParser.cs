using System.Linq;
using Microsoft.AspNetCore.SignalR.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	internal class HubRequestParser : IHubRequestParser
	{
		private class HubInvocation
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

			[JsonProperty("I")]
			public string Id
			{
				get;
				set;
			}

			[JsonProperty("A")]
			public JRaw[] Args
			{
				get;
				set;
			}
		}

		private static readonly IJsonValue[] _emptyArgs = new IJsonValue[0];

		public HubRequest Parse(string data, JsonSerializer serializer)
		{
			HubInvocation hubInvocation = serializer.Parse<HubInvocation>(data);
			return new HubRequest
			{
				Hub = hubInvocation.Hub,
				Method = hubInvocation.Method,
				Id = hubInvocation.Id,
				ParameterValues = ((hubInvocation.Args != null) ? hubInvocation.Args.Select((JRaw value) => new JRawValue(value)).ToArray() : _emptyArgs)
			};
		}
	}
}
