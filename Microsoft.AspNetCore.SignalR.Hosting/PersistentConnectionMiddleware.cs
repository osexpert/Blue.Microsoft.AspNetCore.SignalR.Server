using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SignalR.Hosting
{
	public class PersistentConnectionMiddleware
	{
		private readonly Type _connectionType;

		private readonly IOptions<SignalROptions> _optionsAccessor;

		private readonly RequestDelegate _next;

		private readonly IServiceProvider _serviceProvider;

		public PersistentConnectionMiddleware(RequestDelegate next, Type connectionType, IOptions<SignalROptions> optionsAccessor, IServiceProvider serviceProvider)
		{
			_next = next;
			_serviceProvider = serviceProvider;
			_connectionType = connectionType;
			_optionsAccessor = optionsAccessor;
		}

		public Task Invoke(HttpContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (JsonUtility.TryRejectJSONPRequest(_optionsAccessor.Value, context))
			{
				return TaskAsyncHelper.Empty;
			}
			PersistentConnection obj = ActivatorUtilities.CreateInstance(_serviceProvider, _connectionType) as PersistentConnection;
			obj.Initialize(_serviceProvider);
			return obj.ProcessRequest(context);
		}
	}
}
