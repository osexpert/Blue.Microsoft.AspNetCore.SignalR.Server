using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

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
			_logger = loggerFactory.CreateLogger<ReflectedHubDescriptorProvider>();
		}

		public IList<HubDescriptor> GetHubs()
		{
			return _hubs.Value.Select((KeyValuePair<string, HubDescriptor> kv) => kv.Value).Distinct().ToList();
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
				if (!dictionary.TryGetValue(item.Name, out value))
				{
					dictionary[item.Name] = item;
					continue;
				}
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_DuplicateHubNames, value.HubType.AssemblyQualifiedName, item.HubType.AssemblyQualifiedName, item.Name));
			}
			return dictionary;
		}

		private static bool IsHubType(Type type)
		{
			try
			{
				return TypeExtensions.IsAssignableFrom(typeof(IHub), type) && !type.GetTypeInfo().IsAbstract && (type.GetTypeInfo().Attributes.HasFlag(TypeAttributes.Public) || type.GetTypeInfo().Attributes.HasFlag(TypeAttributes.NestedPublic));
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
				_logger.LogWarning("Some of the classes from assembly \"{0}\" could Not be loaded when searching for Hubs. [{1}]" + Environment.NewLine + "Original exception type: {2}" + Environment.NewLine + "Original exception message: {3}" + Environment.NewLine, a.FullName, null, ((object)ex).GetType().Name, ex.Message);
				if (ex.LoaderExceptions != null)
				{
					_logger.LogWarning("Loader exceptions messages: ");
					Exception[] loaderExceptions = ex.LoaderExceptions;
					foreach (Exception ex2 in loaderExceptions)
					{
						_logger.LogWarning("{0}" + Environment.NewLine, ex2);
					}
				}
				return ex.Types.Where((Type t) => (object)t != null);
			}
			catch (Exception ex3)
			{
				_logger.LogWarning("None of the classes from assembly \"{0}\" could be loaded when searching for Hubs. [{1}]\r\nOriginal exception type: {2}\r\nOriginal exception message: {3}\r\n", a.FullName, null, ((object)ex3).GetType().Name, ex3.Message);
				return Enumerable.Empty<Type>();
			}
		}
	}
}
