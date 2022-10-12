using System;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public interface IBinaryWriter
	{
		void Write(ArraySegment<byte> data);
	}
}
