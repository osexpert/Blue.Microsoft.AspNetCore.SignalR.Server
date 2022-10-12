using Microsoft.AspNetCore.SignalR.Json;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IParameterResolver
	{
		IList<object> ResolveMethodParameters(MethodDescriptor method, IList<IJsonValue> values);
	}
}
