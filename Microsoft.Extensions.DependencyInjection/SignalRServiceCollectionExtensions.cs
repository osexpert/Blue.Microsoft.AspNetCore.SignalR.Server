using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Transports;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class SignalRServiceCollectionExtensions
	{
		public static SignalRServicesBuilder AddSignalR(this IServiceCollection services)
		{
			return services.AddSignalR(null);
		}

		public static SignalRServicesBuilder AddSignalR(this IServiceCollection services, Action<SignalROptions> configureOptions)
		{
			OptionsServiceCollectionExtensions.AddOptions(services);
			DataProtectionServiceCollectionExtensions.AddDataProtection(services);
			services.AddMessageBus();
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IMemoryPool, MemoryPool>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<ITransportManager, TransportManager>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<ITransportHeartbeat, TransportHeartbeat>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IConnectionManager, ConnectionManager>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IAckHandler, AckHandler>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<AckSubscriber, AckSubscriber>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IAssemblyLocator, DefaultAssemblyLocator>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IHubManager, DefaultHubManager>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IMethodDescriptorProvider, ReflectedMethodDescriptorProvider>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IHubDescriptorProvider, ReflectedHubDescriptorProvider>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<JsonSerializer, JsonSerializer>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IUserIdProvider, PrincipalUserIdProvider>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IParameterResolver, DefaultParameterResolver>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IHubActivator, DefaultHubActivator>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IJavaScriptProxyGenerator, DefaultJavaScriptProxyGenerator>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IJavaScriptMinifier, NullJavaScriptMinifier>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IHubRequestParser, HubRequestParser>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IHubPipelineInvoker, HubPipeline>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton(services, typeof(IPersistentConnectionContext<>), typeof(PersistentConnectionContextService<>));
			ServiceCollectionDescriptorExtensions.TryAddSingleton(services, typeof(IHubContext<>), typeof(HubContextService<>));
			ServiceCollectionDescriptorExtensions.TryAddSingleton(services, typeof(IHubContext<, >), typeof(HubContextService<, >));
			ServiceCollectionDescriptorExtensions.TryAddSingleton<SignalRMarkerService, SignalRMarkerService>(services);
			ServiceCollectionDescriptorExtensions.TryAddSingleton<IProtectedData, DataProtectionProviderProtectedData>(services);
			ServiceCollectionDescriptorExtensions.TryAddTransient<IConfigureOptions<SignalROptions>, SignalROptionsSetup>(services);
			if (configureOptions != null)
			{
				OptionsServiceCollectionExtensions.Configure<SignalROptions>(services, configureOptions);
			}
			return new SignalRServicesBuilder(services);
		}
	}
}
