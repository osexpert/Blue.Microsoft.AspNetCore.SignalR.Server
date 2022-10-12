using System;
using System.IO;
using System.Text;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public class MemoryPoolTextWriter : TextWriter
	{
		private readonly IMemoryPool _memory;

		private char[] _textArray;

		private int _textBegin;

		private int _textEnd;

		private const int _textLength = 128;

		private byte[] _dataArray;

		private int _dataEnd;

		private readonly Encoder _encoder;

		public ArraySegment<byte> Buffer => new ArraySegment<byte>(_dataArray, 0, _dataEnd);

		public override Encoding Encoding => Encoding.UTF8;

		public MemoryPoolTextWriter(IMemoryPool memory)
		{
			_memory = memory;
			_textArray = _memory.AllocChar(128);
			_dataArray = _memory.Empty;
			_encoder = Encoding.UTF8.GetEncoder();
			NewLine = "\n";
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (_textArray != null)
					{
						_memory.FreeChar(_textArray);
						_textArray = null;
					}
					if (_dataArray != null)
					{
						_memory.FreeByte(_dataArray);
						_dataArray = null;
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		private void Encode(bool flush)
		{
			int byteCount = _encoder.GetByteCount(_textArray, _textBegin, _textEnd - _textBegin, flush);
			Grow(byteCount);
			int bytes = _encoder.GetBytes(_textArray, _textBegin, _textEnd - _textBegin, _dataArray, _dataEnd, flush);
			_textBegin = (_textEnd = 0);
			_dataEnd += bytes;
		}

		protected void Grow(int minimumAvailable)
		{
			if (_dataArray.Length - _dataEnd < minimumAvailable)
			{
				int minimumSize = _dataArray.Length + Math.Max(_dataArray.Length, minimumAvailable);
				byte[] array = _memory.AllocByte(minimumSize);
				Array.Copy(_dataArray, 0, array, 0, _dataEnd);
				_memory.FreeByte(_dataArray);
				_dataArray = array;
			}
		}

		public override void Write(char value)
		{
			if (128 == _textEnd)
			{
				Encode(false);
				if (128 == _textEnd)
				{
					throw new InvalidOperationException("Unexplainable failure to encode text");
				}
			}
			_textArray[_textEnd++] = value;
		}

		public override void Write(char[] value, int index, int length)
		{
			int num = index;
			int num2 = index + length;
			while (num < num2)
			{
				if (128 == _textEnd)
				{
					Encode(false);
				}
				int num3 = num2 - num;
				if (num3 > 128 - _textEnd)
				{
					num3 = 128 - _textEnd;
				}
				Array.Copy(value, num, _textArray, _textEnd, num3);
				num += num3;
				_textEnd += num3;
			}
		}

		public override void Write(string value)
		{
			int num = 0;
			int length = value.Length;
			while (num < length)
			{
				if (128 == _textEnd)
				{
					Encode(false);
				}
				int num2 = length - num;
				if (num2 > 128 - _textEnd)
				{
					num2 = 128 - _textEnd;
				}
				value.CopyTo(num, _textArray, _textEnd, num2);
				num += num2;
				_textEnd += num2;
			}
		}

		public override void Flush()
		{
			while (_textBegin != _textEnd)
			{
				Encode(true);
			}
		}

		public void Write(ArraySegment<byte> data)
		{
			Flush();
			Grow(data.Count);
			System.Buffer.BlockCopy(data.Array, data.Offset, _dataArray, _dataEnd, data.Count);
			_dataEnd += data.Count;
		}
	}
}
