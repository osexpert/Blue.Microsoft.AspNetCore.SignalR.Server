using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public class ConnectionManager : IConnectionManager
	{
		private readonly IServiceProvider _serviceProvider;

		private readonly IPerformanceCounterManager _counters;

		public ConnectionManager(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_counters = ServiceProviderServiceExtensions.GetRequiredService<IPerformanceCounterManager>(_serviceProvider);
		}

		public IPersistentConnectionContext GetConnectionContext<TConnection>() where TConnection : PersistentConnection
		{
			return GetConnectionContext(typeof(TConnection));
		}

		public IPersistentConnectionContext GetConnectionContext(Type type)
		{
			if ((object)type == null)
			{
				throw new ArgumentNullException("type");
			}
			string fullName = type.FullName;
			string persistentConnectionName = PrefixHelper.GetPersistentConnectionName(fullName);
			Connection connectionCore = GetConnectionCore(persistentConnectionName);
			return new PersistentConnectionContext(connectionCore, new GroupManager(connectionCore, PrefixHelper.GetPersistentConnectionGroupName(fullName)));
		}

		public IHubContext GetHubContext<THub>() where THub : IHub
		{
			return GetHubContext(typeof(THub).GetHubName());
		}

		public IHubContext GetHubContext(string hubName)
		{
			Connection connectionCore = GetConnectionCore(null);
			IHubManager requiredService = ServiceProviderServiceExtensions.GetRequiredService<IHubManager>(_serviceProvider);
			IHubPipelineInvoker requiredService2 = ServiceProviderServiceExtensions.GetRequiredService<IHubPipelineInvoker>(_serviceProvider);
			requiredService.EnsureHub(hubName, _counters.ErrorsHubResolutionTotal, _counters.ErrorsHubResolutionPerSec, _counters.ErrorsAllTotal, _counters.ErrorsAllPerSec);
			return new HubContext(connectionCore, requiredService2, hubName);
		}

		public IHubContext<THub, TClient> GetHubContext<THub, TClient>() where THub : IHub where TClient : class
		{
			return new HubContext<THub, TClient>(GetHubContext<THub>());
		}

		internal Connection GetConnectionCore(string connectionName)
		{
			object list2;
			if (connectionName != null)
			{
				IList<string> list = new string[1]
				{
					connectionName
				};
				list2 = list;
			}
			else
			{
				list2 = Microsoft.AspNetCore.SignalR.Infrastructure.ListHelper<string>.Empty;
			}
			IList<string> signals = (IList<string>)list2;
			ServiceProviderServiceExtensions.GetRequiredService<AckSubscriber>(_serviceProvider);
			string connectionId = Guid.NewGuid().ToString();
			return new Connection(ServiceProviderServiceExtensions.GetRequiredService<IMessageBus>(_serviceProvider), ServiceProviderServiceExtensions.GetRequiredService<JsonSerializer>(_serviceProvider), connectionName, connectionId, signals, Microsoft.AspNetCore.SignalR.Infrastructure.ListHelper<string>.Empty, ServiceProviderServiceExtensions.GetRequiredService<ILoggerFactory>(_serviceProvider), ServiceProviderServiceExtensions.GetRequiredService<IAckHandler>(_serviceProvider), ServiceProviderServiceExtensions.GetRequiredService<IPerformanceCounterManager>(_serviceProvider), ServiceProviderServiceExtensions.GetRequiredService<IProtectedData>(_serviceProvider), ServiceProviderServiceExtensions.GetRequiredService<IMemoryPool>(_serviceProvider));
		}
	}
}
