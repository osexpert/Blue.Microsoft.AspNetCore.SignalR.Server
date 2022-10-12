using System;
using Microsoft.AspNetCore.SignalR.Hubs;

namespace Microsoft.AspNetCore.SignalR
{
	public static class HubOptionsExtensions
	{
		public static void RequireAuthentication(this HubOptions options)
		{
			if (options == null)
			{
				throw new ArgumentNullException("options");
			}
			AuthorizeAttribute authorizeAttribute = new AuthorizeAttribute();
			options.PipelineModules.Add(new AuthorizeModule(authorizeAttribute, authorizeAttribute));
		}
	}
}
