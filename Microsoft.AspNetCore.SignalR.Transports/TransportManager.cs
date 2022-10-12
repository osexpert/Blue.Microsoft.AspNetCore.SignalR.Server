using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;

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
			TransportType enabledTransports = optionsAccessor.get_Value().Transports.EnabledTransports;
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
			_transports.TryRemove(transportName, out Func<HttpContext, ITransport> _);
		}

		public ITransport GetTransport(HttpContext context)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			string text = StringValues.op_Implicit(context.get_Request().get_Query().get_Item("transport"));
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			if (_transports.TryGetValue(text, out Func<HttpContext, ITransport> value))
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
