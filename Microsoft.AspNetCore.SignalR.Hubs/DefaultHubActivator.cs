using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class DefaultHubActivator : IHubActivator
	{
		private readonly IServiceProvider _serviceProvider;

		public DefaultHubActivator(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public IHub Create(HubDescriptor descriptor)
		{
			if (descriptor == null)
			{
				throw new ArgumentNullException("descriptor");
			}
			if ((object)descriptor.HubType == null)
			{
				return null;
			}
			return ActivatorUtilities.CreateInstance(_serviceProvider, descriptor.HubType) as IHub;
		}
	}
}
