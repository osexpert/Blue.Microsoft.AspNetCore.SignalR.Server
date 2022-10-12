using System;

namespace Microsoft.AspNetCore.SignalR.Json
{
	public interface IJsonValue
	{
		object ConvertTo(Type type);

		bool CanConvertTo(Type type);
	}
}
