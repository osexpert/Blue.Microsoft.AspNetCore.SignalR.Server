using Microsoft.AspNetCore.SignalR.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class ReflectedMethodDescriptorProvider : IMethodDescriptorProvider
	{
		private readonly ConcurrentDictionary<string, IDictionary<string, IEnumerable<MethodDescriptor>>> _methods;

		private readonly ConcurrentDictionary<string, MethodDescriptor> _executableMethods;

		public ReflectedMethodDescriptorProvider()
		{
			_methods = new ConcurrentDictionary<string, IDictionary<string, IEnumerable<MethodDescriptor>>>(StringComparer.OrdinalIgnoreCase);
			_executableMethods = new ConcurrentDictionary<string, MethodDescriptor>(StringComparer.OrdinalIgnoreCase);
		}

		public IEnumerable<MethodDescriptor> GetMethods(HubDescriptor hub)
		{
			return FetchMethodsFor(hub).SelectMany((KeyValuePair<string, IEnumerable<MethodDescriptor>> kv) => kv.Value).ToList();
		}

		private IDictionary<string, IEnumerable<MethodDescriptor>> FetchMethodsFor(HubDescriptor hub)
		{
			return _methods.GetOrAdd(hub.Name, (string key) => BuildMethodCacheFor(hub));
		}

		private static IDictionary<string, IEnumerable<MethodDescriptor>> BuildMethodCacheFor(HubDescriptor hub)
		{
			return ReflectionHelper.GetExportedHubMethods(hub.HubType).GroupBy(GetMethodName, StringComparer.OrdinalIgnoreCase).ToDictionary((IGrouping<string, MethodInfo> group) => group.Key, (IGrouping<string, MethodInfo> group) => from oload in @group
			select GetMethodDescriptor(@group.Key, hub, oload), StringComparer.OrdinalIgnoreCase);
		}

		private static MethodDescriptor GetMethodDescriptor(string methodName, HubDescriptor hub, MethodInfo methodInfo)
		{
			Type progressReportingType;
			IEnumerable<ParameterInfo> source = ExtractProgressParameter(methodInfo.GetParameters(), out progressReportingType);
			return new MethodDescriptor
			{
				ReturnType = methodInfo.ReturnType,
				Name = methodName,
				NameSpecified = (GetMethodAttributeName(methodInfo) != null),
				Invoker = new HubMethodDispatcher(methodInfo).Execute,
				Hub = hub,
				Attributes = CustomAttributeExtensions.GetCustomAttributes(methodInfo, typeof(Attribute), true).Cast<Attribute>(),
				ProgressReportingType = progressReportingType,
				Parameters = (from p in source
				select new ParameterDescriptor
				{
					Name = p.Name,
					ParameterType = p.ParameterType
				}).ToList()
			};
		}

		private static IEnumerable<ParameterInfo> ExtractProgressParameter(ParameterInfo[] parameters, out Type progressReportingType)
		{
			ParameterInfo parameterInfo = parameters.LastOrDefault();
			progressReportingType = null;
			if (IsProgressType(parameterInfo))
			{
				progressReportingType = parameterInfo.ParameterType.GenericTypeArguments[0];
				return parameters.Take(parameters.Length - 1);
			}
			return parameters;
		}

		private static bool IsProgressType(ParameterInfo parameter)
		{
			if (parameter != null && parameter.ParameterType.GetTypeInfo().get_IsGenericType())
			{
				return (object)parameter.ParameterType.GetGenericTypeDefinition() == typeof(IProgress<>);
			}
			return false;
		}

		public bool TryGetMethod(HubDescriptor hub, string method, out MethodDescriptor descriptor, IList<IJsonValue> parameters)
		{
			string key = BuildHubExecutableMethodCacheKey(hub, method, parameters);
			if (!_executableMethods.TryGetValue(key, out descriptor))
			{
				if (FetchMethodsFor(hub).TryGetValue(method, out IEnumerable<MethodDescriptor> value))
				{
					List<MethodDescriptor> list = (from o in value
					where o.Matches(parameters)
					select o).ToList();
					descriptor = ((list.Count == 1) ? list[0] : null);
				}
				else
				{
					descriptor = null;
				}
				if (descriptor != null)
				{
					_executableMethods.TryAdd(key, descriptor);
				}
			}
			return descriptor != null;
		}

		private static string BuildHubExecutableMethodCacheKey(HubDescriptor hub, string method, IList<IJsonValue> parameters)
		{
			string text = (parameters == null) ? "0" : parameters.Count.ToString(CultureInfo.InvariantCulture);
			string text2 = method.ToUpperInvariant();
			return hub.Name + "::" + text2 + "(" + text + ")";
		}

		private static string GetMethodName(MethodInfo method)
		{
			return GetMethodAttributeName(method) ?? method.Name;
		}

		private static string GetMethodAttributeName(MethodInfo method)
		{
			return ReflectionHelper.GetAttributeValue(method, (HubMethodNameAttribute a) => a.MethodName);
		}
	}
}
