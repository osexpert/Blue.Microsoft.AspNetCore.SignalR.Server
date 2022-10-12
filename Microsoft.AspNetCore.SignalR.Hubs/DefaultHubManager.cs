using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Json;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class DefaultHubManager : IHubManager
	{
		private readonly IEnumerable<IMethodDescriptorProvider> _methodProviders;

		private readonly IHubActivator _activator;

		private readonly IEnumerable<IHubDescriptorProvider> _hubProviders;

		public DefaultHubManager(IEnumerable<IHubDescriptorProvider> hubProviders, IEnumerable<IMethodDescriptorProvider> methodProviders, IHubActivator activator)
		{
			_hubProviders = hubProviders;
			_methodProviders = methodProviders;
			_activator = activator;
		}

		public HubDescriptor GetHub(string hubName)
		{
			HubDescriptor descriptor = null;
			if (_hubProviders.FirstOrDefault((IHubDescriptorProvider p) => p.TryGetHub(hubName, out descriptor)) != null)
			{
				return descriptor;
			}
			return null;
		}

		public IEnumerable<HubDescriptor> GetHubs(Func<HubDescriptor, bool> predicate)
		{
			IEnumerable<HubDescriptor> enumerable = _hubProviders.SelectMany((IHubDescriptorProvider p) => p.GetHubs());
			if (predicate != null)
			{
				return enumerable.Where(predicate);
			}
			return enumerable;
		}

		public MethodDescriptor GetHubMethod(string hubName, string method, IList<IJsonValue> parameters)
		{
			HubDescriptor hub = GetHub(hubName);
			if (hub == null)
			{
				return null;
			}
			MethodDescriptor descriptor = null;
			if (_methodProviders.FirstOrDefault((IMethodDescriptorProvider p) => p.TryGetMethod(hub, method, out descriptor, parameters)) != null)
			{
				return descriptor;
			}
			return null;
		}

		public IEnumerable<MethodDescriptor> GetHubMethods(string hubName, Func<MethodDescriptor, bool> predicate)
		{
			HubDescriptor hub = GetHub(hubName);
			if (hub == null)
			{
				return null;
			}
			IEnumerable<MethodDescriptor> enumerable = _methodProviders.SelectMany((IMethodDescriptorProvider p) => p.GetMethods(hub));
			if (predicate != null)
			{
				return enumerable.Where(predicate);
			}
			return enumerable;
		}

		public IHub ResolveHub(string hubName)
		{
			HubDescriptor hub = GetHub(hubName);
			if (hub != null)
			{
				return _activator.Create(hub);
			}
			return null;
		}

		public IEnumerable<IHub> ResolveHubs()
		{
			return from hub in GetHubs(null)
				select _activator.Create(hub);
		}
	}
}
