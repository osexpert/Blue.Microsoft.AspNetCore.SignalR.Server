using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class ReflectedHubDescriptorProvider : IHubDescriptorProvider
	{
		private readonly Lazy<IDictionary<string, HubDescriptor>> _hubs;

		private readonly IAssemblyLocator _locator;

		private readonly ILogger _logger;

		public ReflectedHubDescriptorProvider(IAssemblyLocator locator, ILoggerFactory loggerFactory)
		{
			_locator = locator;
			_hubs = new Lazy<IDictionary<string, HubDescriptor>>(BuildHubsCache);
			_logger = LoggerFactoryExtensions.CreateLogger<ReflectedHubDescriptorProvider>(loggerFactory);
		}

		public IList<HubDescriptor> GetHubs()
		{
			return (from kv in _hubs.Value
			select kv.Value).Distinct().ToList();
		}

		public bool TryGetHub(string hubName, out HubDescriptor descriptor)
		{
			return _hubs.Value.TryGetValue(hubName, out descriptor);
		}

		protected IDictionary<string, HubDescriptor> BuildHubsCache()
		{
			IEnumerable<HubDescriptor> enumerable = from type in _locator.GetAssemblies().SelectMany(GetTypesSafe).Where(IsHubType)
			select new HubDescriptor
			{
				NameSpecified = (type.GetHubAttributeName() != null),
				Name = type.GetHubName(),
				HubType = type
			};
			Dictionary<string, HubDescriptor> dictionary = new Dictionary<string, HubDescriptor>(StringComparer.OrdinalIgnoreCase);
			foreach (HubDescriptor item in enumerable)
			{
				HubDescriptor value = null;
				if (dictionary.TryGetValue(item.Name, out value))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_DuplicateHubNames, value.HubType.AssemblyQualifiedName, item.HubType.AssemblyQualifiedName, item.Name));
				}
				dictionary[item.Name] = item;
			}
			return dictionary;
		}

		private static bool IsHubType(Type type)
		{
			try
			{
				return TypeExtensions.IsAssignableFrom(typeof(IHub), type) && !type.GetTypeInfo().get_IsAbstract() && (type.GetTypeInfo().get_Attributes().HasFlag(TypeAttributes.Public) || type.GetTypeInfo().get_Attributes().HasFlag(TypeAttributes.NestedPublic));
			}
			catch
			{
				return false;
			}
		}

		private IEnumerable<Type> GetTypesSafe(Assembly a)
		{
			try
			{
				return a.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				LoggerExtensions.LogWarning(_logger, "Some of the classes from assembly \"{0}\" could Not be loaded when searching for Hubs. [{1}]" + Environment.NewLine + "Original exception type: {2}" + Environment.NewLine + "Original exception message: {3}" + Environment.NewLine, new object[4]
				{
					a.FullName,
					null,
					((object)ex).GetType().get_Name(),
					ex.Message
				});
				if (ex.LoaderExceptions != null)
				{
					LoggerExtensions.LogWarning(_logger, "Loader exceptions messages: ", Array.Empty<object>());
					Exception[] loaderExceptions = ex.LoaderExceptions;
					foreach (Exception ex2 in loaderExceptions)
					{
						LoggerExtensions.LogWarning(_logger, "{0}" + Environment.NewLine, new object[1]
						{
							ex2
						});
					}
				}
				return from t in ex.Types
				where (object)t != null
				select t;
			}
			catch (Exception ex3)
			{
				LoggerExtensions.LogWarning(_logger, "None of the classes from assembly \"{0}\" could be loaded when searching for Hubs. [{1}]\r\nOriginal exception type: {2}\r\nOriginal exception message: {3}\r\n", new object[4]
				{
					a.FullName,
					null,
					((object)ex3).GetType().get_Name(),
					ex3.Message
				});
				return Enumerable.Empty<Type>();
			}
		}
	}
}
