using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Json;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IParameterResolver
	{
		IList<object> ResolveMethodParameters(MethodDescriptor method, IList<IJsonValue> values);
	}
}
