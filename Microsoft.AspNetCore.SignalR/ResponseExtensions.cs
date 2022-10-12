using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR
{
	internal static class ResponseExtensions
	{
		public static void Write(this HttpResponse response, ArraySegment<byte> data)
		{
			response.get_Body().Write(data.Array, data.Offset, data.Count);
		}

		public static Task Flush(this HttpResponse response)
		{
			return response.get_Body().FlushAsync();
		}

		public static Task End(this HttpResponse response, string data)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(data);
			response.set_ContentLength((long?)bytes.Length);
			response.get_Body().Write(bytes, 0, bytes.Length);
			return response.get_Body().FlushAsync();
		}
	}
}
