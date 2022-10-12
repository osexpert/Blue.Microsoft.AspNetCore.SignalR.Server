using System;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.SignalR.Json
{
	public static class JsonSerializerExtensions
	{
		public static T Parse<T>(this JsonSerializer serializer, string json)
		{
			if (serializer == null)
			{
				throw new ArgumentNullException("serializer");
			}
			using (StringReader reader = new StringReader(json))
			{
				return (T)serializer.Deserialize(reader, typeof(T));
			}
		}

		public static T Parse<T>(this JsonSerializer serializer, ArraySegment<byte> jsonBuffer, Encoding encoding)
		{
			if (serializer == null)
			{
				throw new ArgumentNullException("serializer");
			}
			using (ArraySegmentTextReader reader = new ArraySegmentTextReader(jsonBuffer, encoding))
			{
				return (T)serializer.Deserialize(reader, typeof(T));
			}
		}

		public static void Serialize(this JsonSerializer serializer, object value, TextWriter writer)
		{
			if (serializer == null)
			{
				throw new ArgumentNullException("serializer");
			}
			IJsonWritable jsonWritable = value as IJsonWritable;
			if (jsonWritable != null)
			{
				jsonWritable.WriteJson(writer);
			}
			else
			{
				serializer.Serialize(writer, value);
			}
		}

		public static string Stringify(this JsonSerializer serializer, object value)
		{
			if (serializer == null)
			{
				throw new ArgumentNullException("serializer");
			}
			using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
			{
				serializer.Serialize(value, stringWriter);
				return stringWriter.ToString();
			}
		}
	}
}
