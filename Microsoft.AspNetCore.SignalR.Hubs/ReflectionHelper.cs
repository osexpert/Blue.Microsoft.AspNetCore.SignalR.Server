using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public static class ReflectionHelper
	{
		private static readonly Type[] _excludeTypes = new Type[2]
		{
			typeof(Hub),
			typeof(object)
		};

		private static readonly Type[] _excludeInterfaces = new Type[2]
		{
			typeof(IHub),
			typeof(IDisposable)
		};

		public static IEnumerable<MethodInfo> GetExportedHubMethods(Type type)
		{
			if (!TypeExtensions.IsAssignableFrom(typeof(IHub), type))
			{
				return Enumerable.Empty<MethodInfo>();
			}
			MethodInfo[] methods = TypeExtensions.GetMethods(type, BindingFlags.Instance | BindingFlags.Public);
			IEnumerable<MethodInfo> second = _excludeInterfaces.SelectMany((Type i) => GetInterfaceMethods(type, i));
			return methods.Except(second).Where(IsValidHubMethod);
		}

		private static bool IsValidHubMethod(MethodInfo methodInfo)
		{
			if (!_excludeTypes.Contains(methodInfo.GetBaseDefinition().DeclaringType))
			{
				return !methodInfo.IsSpecialName;
			}
			return false;
		}

		private static IEnumerable<MethodInfo> GetInterfaceMethods(Type type, Type iface)
		{
			if (!TypeExtensions.IsAssignableFrom(iface, type))
			{
				return Enumerable.Empty<MethodInfo>();
			}
			return RuntimeReflectionExtensions.GetRuntimeInterfaceMap(type.GetTypeInfo(), iface).TargetMethods;
		}

		public static TResult GetAttributeValue<TAttribute, TResult>(MethodInfo methodInfo, Func<TAttribute, TResult> valueGetter) where TAttribute : Attribute
		{
			return GetAttributeValue(() => methodInfo.GetCustomAttribute<TAttribute>(), valueGetter);
		}

		public static TResult GetAttributeValue<TAttribute, TResult>(TypeInfo source, Func<TAttribute, TResult> valueGetter) where TAttribute : Attribute
		{
			return GetAttributeValue(() => source.GetCustomAttribute<TAttribute>(), valueGetter);
		}

		public static TResult GetAttributeValue<TAttribute, TResult>(Func<TAttribute> attributeProvider, Func<TAttribute, TResult> valueGetter) where TAttribute : Attribute
		{
			if (attributeProvider == null)
			{
				throw new ArgumentNullException("attributeProvider");
			}
			if (valueGetter == null)
			{
				throw new ArgumentNullException("valueGetter");
			}
			TAttribute val = attributeProvider();
			if (val != null)
			{
				return valueGetter(val);
			}
			return default(TResult);
		}
	}
}
