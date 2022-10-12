using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SignalR
{
	public interface IUserIdProvider
	{
		string GetUserId(HttpRequest request);
	}
}
