using System;

namespace Microsoft.Extensions.DependencyInjection
{
	public class SignalRServicesBuilder
	{
		private readonly IServiceCollection _serviceCollection;

		public virtual IServiceCollection ServiceCollection => _serviceCollection;

		public SignalRServicesBuilder(IServiceCollection serviceCollection)
		{
			if (serviceCollection == null)
			{
				throw new ArgumentNullException("serviceCollection");
			}
			_serviceCollection = serviceCollection;
		}
	}
}
