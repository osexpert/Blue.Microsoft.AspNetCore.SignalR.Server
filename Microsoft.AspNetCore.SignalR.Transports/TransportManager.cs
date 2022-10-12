using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	public class TransportManager : ITransportManager
	{
		private readonly ConcurrentDictionary<string, Func<HttpContext, ITransport>> _transports = new ConcurrentDictionary<string, Func<HttpContext, ITransport>>(StringComparer.OrdinalIgnoreCase);

		public TransportManager(IServiceProvider serviceProvider, IOptions<SignalROptions> optionsAccessor)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			if (optionsAccessor == null)
			{
				throw new ArgumentNullException("optionsAccessor");
			}
			TransportType enabledTransports = optionsAccessor.Value.Transports.EnabledTransports;
			if (enabledTransports.HasFlag(TransportType.WebSockets))
			{
				Register("webSockets", (HttpContext context) => ActivatorUtilities.CreateInstance<WebSocketTransport>(serviceProvider, new object[1]
				{
					context
				}));
			}
			if (enabledTransports.HasFlag(TransportType.ServerSentEvents))
			{
				Register("serverSentEvents", (HttpContext context) => ActivatorUtilities.CreateInstance<ServerSentEventsTransport>(serviceProvider, new object[1]
				{
					context
				}));
			}
			if (enabledTransports.HasFlag(TransportType.LongPolling))
			{
				Register("longPolling", (HttpContext context) => ActivatorUtilities.CreateInstance<LongPollingTransport>(serviceProvider, new object[1]
				{
					context
				}));
			}
			if (_transports.Count == 0)
			{
				throw new InvalidOperationException(Resources.Error_NoTransportsEnabled);
			}
		}

		public void Register(string transportName, Func<HttpContext, ITransport> transportFactory)
		{
			if (string.IsNullOrEmpty(transportName))
			{
				throw new ArgumentNullException("transportName");
			}
			if (transportFactory == null)
			{
				throw new ArgumentNullException("transportFactory");
			}
			_transports.TryAdd(transportName, transportFactory);
		}

		public void Remove(string transportName)
		{
			if (string.IsNullOrEmpty(transportName))
			{
				throw new ArgumentNullException("transportName");
			}
			_transports.TryRemove(transportName, out var _);
		}

		public ITransport GetTransport(HttpContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			string text = context.Request.Query["transport"];
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			if (_transports.TryGetValue(text, out var value))
			{
				return value(context);
			}
			return null;
		}

		public bool SupportsTransport(string transportName)
		{
			if (string.IsNullOrEmpty(transportName))
			{
				return false;
			}
			return _transports.ContainsKey(transportName);
		}
	}
}
