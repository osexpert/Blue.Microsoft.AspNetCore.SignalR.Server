using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Json;
using Microsoft.AspNetCore.SignalR.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	public sealed class PersistentResponse : IJsonWritable
	{
		private readonly Func<Message, bool> _exclude;

		private readonly Action<TextWriter> _writeCursor;

		public IList<ArraySegment<Message>> Messages
		{
			get;
			set;
		}

		public bool Terminal
		{
			get;
			set;
		}

		public int TotalCount
		{
			get;
			set;
		}

		public bool Initializing
		{
			get;
			set;
		}

		public bool Aborted
		{
			get;
			set;
		}

		public bool Reconnect
		{
			get;
			set;
		}

		public string GroupsToken
		{
			get;
			set;
		}

		public long? LongPollDelay
		{
			get;
			set;
		}

		public PersistentResponse()
			: this((Message message) => false, delegate
			{
			})
		{
		}

		public PersistentResponse(Func<Message, bool> exclude, Action<TextWriter> writeCursor)
		{
			_exclude = exclude;
			_writeCursor = writeCursor;
		}

		void IJsonWritable.WriteJson(TextWriter writer)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Expected O, but got Unknown
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			JsonTextWriter val = new JsonTextWriter(writer);
			val.WriteStartObject();
			writer.Write('"');
			writer.Write("C");
			writer.Write('"');
			writer.Write(':');
			writer.Write('"');
			_writeCursor(writer);
			writer.Write('"');
			writer.Write(',');
			if (Initializing)
			{
				val.WritePropertyName("S");
				val.WriteValue(1);
			}
			if (Reconnect)
			{
				val.WritePropertyName("T");
				val.WriteValue(1);
			}
			if (GroupsToken != null)
			{
				val.WritePropertyName("G");
				val.WriteValue(GroupsToken);
			}
			if (LongPollDelay.HasValue)
			{
				val.WritePropertyName("L");
				val.WriteValue(LongPollDelay.Value);
			}
			val.WritePropertyName("M");
			val.WriteStartArray();
			WriteMessages(writer, val);
			val.WriteEndArray();
			val.WriteEndObject();
		}

		private void WriteMessages(TextWriter writer, JsonTextWriter jsonWriter)
		{
			if (Messages != null)
			{
				IBinaryWriter binaryWriter = writer as IBinaryWriter;
				bool flag = true;
				for (int i = 0; i < Messages.Count; i++)
				{
					ArraySegment<Message> arraySegment = Messages[i];
					for (int j = arraySegment.Offset; j < arraySegment.Offset + arraySegment.Count; j++)
					{
						Message message = arraySegment.Array[j];
						if (!message.IsCommand && !_exclude(message))
						{
							if (binaryWriter != null)
							{
								if (!flag)
								{
									writer.Write(',');
								}
								binaryWriter.Write(message.Value);
								flag = false;
							}
							else
							{
								jsonWriter.WriteRawValue(message.GetString());
							}
						}
					}
				}
			}
		}
	}
}
