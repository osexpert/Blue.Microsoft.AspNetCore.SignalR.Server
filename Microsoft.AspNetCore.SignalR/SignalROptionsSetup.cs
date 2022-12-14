using System;
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SignalR
{
	public class SignalROptionsSetup : ConfigureOptions<SignalROptions>
	{
		public SignalROptionsSetup()
			: base((Action<SignalROptions>)ConfigureSignalR)
		{
		}

		private static void ConfigureSignalR(SignalROptions options)
		{
			options.Hubs.PipelineModules.Add(new AuthorizeModule());
		}
	}
}
