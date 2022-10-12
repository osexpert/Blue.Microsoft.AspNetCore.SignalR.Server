using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SignalR.Transports
{
	public interface ITransportManager
	{
		ITransport GetTransport(HttpContext context);

		bool SupportsTransport(string transportName);
	}
}
