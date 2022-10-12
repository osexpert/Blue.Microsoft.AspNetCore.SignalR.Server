using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SignalR
{
	internal static class ResponseExtensions
	{
		public static void Write(this HttpResponse response, ArraySegment<byte> data)
		{
			response.Body.Write(data.Array, data.Offset, data.Count);
		}

        public static Task WriteAsync(this HttpResponse response, ArraySegment<byte> data)
        {
            return response.Body.WriteAsync(data.Array, data.Offset, data.Count);
        }

        public static Task Flush(this HttpResponse response)
		{
			return response.Body.FlushAsync();
		}

		public static async Task End(this HttpResponse response, string data)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(data);
			response.ContentLength = bytes.Length;
			await response.Body.WriteAsync(bytes, 0, bytes.Length);
			await response.Body.FlushAsync();
		}
	}
}
