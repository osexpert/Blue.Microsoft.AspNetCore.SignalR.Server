using Microsoft.AspNetCore.SignalR.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public static class MethodExtensions
	{
		public static bool Matches(this MethodDescriptor methodDescriptor, IList<IJsonValue> parameters)
		{
			if (methodDescriptor == null)
			{
				throw new ArgumentNullException("methodDescriptor");
			}
			if ((methodDescriptor.Parameters.Count > 0 && parameters == null) || methodDescriptor.Parameters.Count != parameters.Count)
			{
				return false;
			}
			return true;
		}
	}
}
