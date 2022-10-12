using System;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public interface IMemoryPool
	{
		byte[] Empty
		{
			get;
		}

		byte[] AllocByte(int minimumSize);

		void FreeByte(byte[] memory);

		char[] AllocChar(int minimumSize);

		void FreeChar(char[] memory);

		ArraySegment<byte> AllocSegment(int minimumSize);

		void FreeSegment(ArraySegment<byte> segment);
	}
}
