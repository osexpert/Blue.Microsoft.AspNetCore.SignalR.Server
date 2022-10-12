using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Microsoft.AspNetCore.SignalR.Json
{
	internal class JRawValue : IJsonValue
	{
		private readonly string _value;

		public JRawValue(JRaw value)
		{
			_value = ((object)value).ToString();
		}

		public object ConvertTo(Type type)
		{
			using (StringReader stringReader = new StringReader(_value))
			{
				return JsonUtility.CreateDefaultSerializer().Deserialize((TextReader)stringReader, type);
			}
		}

		public bool CanConvertTo(Type type)
		{
			return true;
		}
	}
}
