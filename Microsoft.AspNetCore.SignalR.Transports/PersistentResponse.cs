using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR.Json;
using Microsoft.AspNetCore.SignalR.Messaging;
using Newtonsoft.Json;

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
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			JsonTextWriter jsonTextWriter = new JsonTextWriter(writer);
			jsonTextWriter.WriteStartObject();
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
				jsonTextWriter.WritePropertyName("S");
				jsonTextWriter.WriteValue(1);
			}
			if (Reconnect)
			{
				jsonTextWriter.WritePropertyName("T");
				jsonTextWriter.WriteValue(1);
			}
			if (GroupsToken != null)
			{
				jsonTextWriter.WritePropertyName("G");
				jsonTextWriter.WriteValue(GroupsToken);
			}
			if (LongPollDelay.HasValue)
			{
				jsonTextWriter.WritePropertyName("L");
				jsonTextWriter.WriteValue(LongPollDelay.Value);
			}
			jsonTextWriter.WritePropertyName("M");
			jsonTextWriter.WriteStartArray();
			WriteMessages(writer, jsonTextWriter);
			jsonTextWriter.WriteEndArray();
			jsonTextWriter.WriteEndObject();
		}

		private void WriteMessages(TextWriter writer, JsonTextWriter jsonWriter)
		{
			if (Messages == null)
			{
				return;
			}
			IBinaryWriter binaryWriter = writer as IBinaryWriter;
			bool flag = true;
			for (int i = 0; i < Messages.Count; i++)
			{
				ArraySegment<Message> arraySegment = Messages[i];
				for (int j = arraySegment.Offset; j < arraySegment.Offset + arraySegment.Count; j++)
				{
					Message message = arraySegment.Array[j];
					if (message.IsCommand || _exclude(message))
					{
						continue;
					}
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
