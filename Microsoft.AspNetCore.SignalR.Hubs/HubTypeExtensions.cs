using System;
using System.Reflection;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	internal static class HubTypeExtensions
	{
		internal static string GetHubName(this Type type)
		{
			if (!typeof(IHub).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
			{
				return null;
			}
			return type.GetHubAttributeName() ?? GetHubTypeName(type);
		}

		internal static string GetHubAttributeName(this Type type)
		{
			if (!typeof(IHub).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
			{
				return null;
			}
			return ReflectionHelper.GetAttributeValue(type.GetTypeInfo(), (HubNameAttribute attr) => attr.HubName);
		}

		private static string GetHubTypeName(Type type)
		{
			int num = type.get_Name().LastIndexOf('`');
			if (num == -1)
			{
				return type.get_Name();
			}
			return type.get_Name().Substring(0, num);
		}
	}
}
