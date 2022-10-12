using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal static class StreamExtensions
	{
		public static Task<int> ReadAsync(this Stream stream, byte[] buffer)
		{
			return stream.ReadAsync(buffer, 0, buffer.Length);
		}

		public static Task WriteAsync(this Stream stream, byte[] buffer)
		{
			return stream.WriteAsync(buffer, 0, buffer.Length);
		}
	}
}
