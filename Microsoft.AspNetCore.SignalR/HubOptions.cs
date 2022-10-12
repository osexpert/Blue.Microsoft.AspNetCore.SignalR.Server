using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Hubs;

namespace Microsoft.AspNetCore.SignalR
{
	public class HubOptions
	{
		public bool EnableJavaScriptProxies
		{
			get;
			set;
		}

		public bool EnableDetailedErrors
		{
			get;
			set;
		}

		public List<IHubPipelineModule> PipelineModules
		{
			get;
		} = new List<IHubPipelineModule>();


		public HubOptions()
		{
			EnableJavaScriptProxies = true;
		}
	}
}
