using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.SignalR.Json
{
	internal class JRawValue : IJsonValue
	{
		private readonly string _value;

		public JRawValue(JRaw value)
		{
			_value = value.ToString();
		}

		public object ConvertTo(Type type)
		{
			using (StringReader reader = new StringReader(_value))
			{
				return JsonUtility.CreateDefaultSerializer().Deserialize(reader, type);
			}
		}

		public bool CanConvertTo(Type type)
		{
			return true;
		}
	}
}
