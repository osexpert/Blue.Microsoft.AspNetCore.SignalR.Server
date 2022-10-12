using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Newtonsoft.Json;

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
			switch (reader.TokenType)
			{
			case JsonToken.StartObject:
				return ReadObject(reader);
			case JsonToken.StartArray:
				return ReadArray(reader);
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.String:
			case JsonToken.Boolean:
			case JsonToken.Null:
			case JsonToken.Undefined:
			case JsonToken.Date:
			case JsonToken.Bytes:
				return reader.Value;
			default:
				throw new NotSupportedException();
			}
		}

		private object ReadArray(JsonReader reader)
		{
			List<object> list = new List<object>();
			while (reader.Read())
			{
				JsonToken tokenType = reader.TokenType;
				if (tokenType != JsonToken.EndArray)
				{
					object item = ReadJsonObject(reader);
					list.Add(item);
					continue;
				}
				return list;
			}
			throw new JsonSerializationException(Resources.Error_ParseObjectFailed);
		}

		private object ReadObject(JsonReader reader)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>(new Microsoft.AspNetCore.SignalR.Infrastructure.SipHashBasedStringEqualityComparer());
			while (reader.Read())
			{
				string key;
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					key = reader.Value.ToString();
					if (!reader.Read())
					{
						throw new JsonSerializationException(Resources.Error_ParseObjectFailed);
					}
					break;
				case JsonToken.EndObject:
					return dictionary;
				default:
					throw new JsonSerializationException(Resources.Error_ParseObjectFailed);
				}
				object obj2 = (dictionary[key] = ReadJsonObject(reader));
			}
			throw new JsonSerializationException(Resources.Error_ParseObjectFailed);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
