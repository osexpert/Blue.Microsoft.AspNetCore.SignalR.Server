using Microsoft.AspNetCore.SignalR.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class DefaultParameterResolver : IParameterResolver
	{
		public virtual object ResolveParameter(ParameterDescriptor descriptor, IJsonValue value)
		{
			if (descriptor == null)
			{
				throw new ArgumentNullException("descriptor");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if ((object)value.GetType() == descriptor.ParameterType)
			{
				return value;
			}
			return value.ConvertTo(descriptor.ParameterType);
		}

		public virtual IList<object> ResolveMethodParameters(MethodDescriptor method, IList<IJsonValue> values)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			return method.Parameters.Zip(values, ResolveParameter).ToArray();
		}
	}
}
