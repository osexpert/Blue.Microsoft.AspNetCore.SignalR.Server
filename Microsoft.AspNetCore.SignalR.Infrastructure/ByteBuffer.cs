using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal sealed class ByteBuffer
	{
		private int _currentLength;

		private readonly int? _maxLength;

		private readonly List<byte[]> _segments = new List<byte[]>();

		public ByteBuffer(int? maxLength)
		{
			_maxLength = maxLength;
		}

		public void Append(byte[] segment)
		{
			checked
			{
				_currentLength += segment.Length;
				if (_maxLength.HasValue && _currentLength > _maxLength)
				{
					throw new InvalidOperationException("Buffer length exceeded");
				}
				_segments.Add(segment);
			}
		}

		public byte[] GetByteArray()
		{
			byte[] array = new byte[_currentLength];
			int num = 0;
			for (int i = 0; i < _segments.Count; i++)
			{
				byte[] array2 = _segments[i];
				Buffer.BlockCopy(array2, 0, array, num, array2.Length);
				num += array2.Length;
			}
			return array;
		}

		public string GetString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			Decoder decoder = Encoding.UTF8.GetDecoder();
			for (int i = 0; i < _segments.Count; i++)
			{
				bool flush = i == _segments.Count - 1;
				byte[] array = _segments[i];
				char[] array2 = new char[decoder.GetCharCount(array, 0, array.Length, flush)];
				int chars = decoder.GetChars(array, 0, array.Length, array2, 0, flush);
				stringBuilder.Append(array2, 0, chars);
			}
			return stringBuilder.ToString();
		}
	}
}
