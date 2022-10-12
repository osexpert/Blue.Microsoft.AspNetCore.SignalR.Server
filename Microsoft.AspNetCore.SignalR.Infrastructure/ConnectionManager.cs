using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public class ConnectionManager : IConnectionManager
	{
		private readonly IServiceProvider _serviceProvider;

		private readonly IPerformanceCounterManager _counters;

		public ConnectionManager(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_counters = _serviceProvider.GetRequiredService<IPerformanceCounterManager>();
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
			IHubManager requiredService = _serviceProvider.GetRequiredService<IHubManager>();
			IHubPipelineInvoker requiredService2 = _serviceProvider.GetRequiredService<IHubPipelineInvoker>();
			requiredService.EnsureHub(hubName, _counters.ErrorsHubResolutionTotal, _counters.ErrorsHubResolutionPerSec, _counters.ErrorsAllTotal, _counters.ErrorsAllPerSec);
			return new HubContext(connectionCore, requiredService2, hubName);
		}

		public IHubContext<THub, TClient> GetHubContext<THub, TClient>() where THub : IHub where TClient : class
		{
			return new HubContext<THub, TClient>(GetHubContext<THub>());
		}

		internal Connection GetConnectionCore(string connectionName)
		{
			IList<string> list2;
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
				list2 = ListHelper<string>.Empty;
			}
			IList<string> signals = list2;
			_serviceProvider.GetRequiredService<AckSubscriber>();
			string connectionId = Guid.NewGuid().ToString();
			return new Connection(_serviceProvider.GetRequiredService<IMessageBus>(), _serviceProvider.GetRequiredService<JsonSerializer>(), connectionName, connectionId, signals, ListHelper<string>.Empty, _serviceProvider.GetRequiredService<ILoggerFactory>(), _serviceProvider.GetRequiredService<IAckHandler>(), _serviceProvider.GetRequiredService<IPerformanceCounterManager>(), _serviceProvider.GetRequiredService<IProtectedData>(), _serviceProvider.GetRequiredService<IMemoryPool>());
		}
	}
}
