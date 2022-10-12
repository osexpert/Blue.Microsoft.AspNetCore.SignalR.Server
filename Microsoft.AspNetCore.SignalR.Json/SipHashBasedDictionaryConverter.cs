using Microsoft.AspNetCore.SignalR.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Json
{
	internal class SipHashBasedDictionaryConverter : JsonConverter
	{
		public override bool CanWrite => false;

		public override bool CanConvert(Type objectType)
		{
			return (object)objectType == typeof(IDictionary<string, object>);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return ReadJsonObject(reader);
		}

		private object ReadJsonObject(JsonReader reader)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Expected I4, but got Unknown
			JsonToken tokenType = reader.get_TokenType();
			switch (tokenType - 1)
			{
			case 0:
				return ReadObject(reader);
			case 1:
				return ReadArray(reader);
			case 6:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 15:
			case 16:
				return reader.get_Value();
			default:
				throw new NotSupportedException();
			}
		}

		private object ReadArray(JsonReader reader)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I4
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			List<object> list = new List<object>();
			while (reader.Read())
			{
				JsonToken tokenType = reader.get_TokenType();
				if ((int)tokenType == 14)
				{
					return list;
				}
				object item = ReadJsonObject(reader);
				list.Add(item);
			}
			throw new JsonSerializationException(Resources.Error_ParseObjectFailed);
		}

		private object ReadObject(JsonReader reader)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Invalid comparison between Unknown and I4
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Invalid comparison between Unknown and I4
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			Dictionary<string, object> dictionary = new Dictionary<string, object>(new Microsoft.AspNetCore.SignalR.Infrastructure.SipHashBasedStringEqualityComparer());
			while (reader.Read())
			{
				JsonToken tokenType = reader.get_TokenType();
				if ((int)tokenType != 4)
				{
					if ((int)tokenType == 13)
					{
						return dictionary;
					}
					throw new JsonSerializationException(Resources.Error_ParseObjectFailed);
				}
				string key = reader.get_Value().ToString();
				if (!reader.Read())
				{
					throw new JsonSerializationException(Resources.Error_ParseObjectFailed);
				}
				object obj2 = dictionary[key] = ReadJsonObject(reader);
			}
			throw new JsonSerializationException(Resources.Error_ParseObjectFailed);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public SipHashBasedDictionaryConverter()
			: this()
		{
		}
	}
}
