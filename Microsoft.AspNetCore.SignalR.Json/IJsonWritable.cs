using System.IO;

namespace Microsoft.AspNetCore.SignalR.Json
{
	public interface IJsonWritable
	{
		void WriteJson(TextWriter writer);
	}
}
