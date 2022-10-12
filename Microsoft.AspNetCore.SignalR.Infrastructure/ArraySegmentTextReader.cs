using System;
using System.IO;
using System.Text;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public class ArraySegmentTextReader : TextReader
	{
		private readonly ArraySegment<byte> _buffer;

		private readonly Encoding _encoding;

		private int _offset;

		public ArraySegmentTextReader(ArraySegment<byte> buffer, Encoding encoding)
		{
			_buffer = buffer;
			_encoding = encoding;
			_offset = _buffer.Offset;
		}

		public override int Read(char[] buffer, int index, int count)
		{
			int byteCount = _encoding.GetByteCount(buffer, index, count);
			int num = Math.Min(_buffer.Count - _offset, byteCount);
			int chars = _encoding.GetChars(_buffer.Array, _offset, num, buffer, index);
			_offset += num;
			return chars;
		}
	}
}
