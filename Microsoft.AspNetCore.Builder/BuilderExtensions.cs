using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hosting;
using Microsoft.AspNetCore.SignalR.Hubs;

namespace Microsoft.AspNetCore.Builder
{
	public static class BuilderExtensions
	{
		public static IApplicationBuilder UseSignalR(this IApplicationBuilder builder)
		{
			return builder.UseSignalR("/signalr");
		}

		public static IApplicationBuilder UseSignalR(this IApplicationBuilder builder, string path)
		{
			return builder.Map(path, delegate(IApplicationBuilder subApp)
			{
				subApp.RunSignalR();
			});
		}

		public static void RunSignalR(this IApplicationBuilder builder)
		{
			builder.RunSignalR(typeof(HubDispatcher));
		}

		public static IApplicationBuilder UseSignalR<TConnection>(this IApplicationBuilder builder, string path) where TConnection : PersistentConnection
		{
			return builder.UseSignalR(path, typeof(TConnection));
		}

		public static IApplicationBuilder UseSignalR(this IApplicationBuilder builder, string path, Type connectionType)
		{
			return builder.Map(path, delegate(IApplicationBuilder subApp)
			{
				subApp.RunSignalR(connectionType);
			});
		}

		public static void RunSignalR<TConnection>(this IApplicationBuilder builder) where TConnection : PersistentConnection
		{
			builder.RunSignalR(typeof(TConnection));
		}

		public static void RunSignalR(this IApplicationBuilder builder, Type connectionType)
		{
			if (builder.ApplicationServices.GetService(typeof(SignalRMarkerService)) == null)
			{
				throw new InvalidOperationException(Resources.Error_ServicesNotRegistered);
			}
			builder.UseMiddleware<PersistentConnectionMiddleware>(new object[1]
			{
				connectionType
			});
		}
	}
}
