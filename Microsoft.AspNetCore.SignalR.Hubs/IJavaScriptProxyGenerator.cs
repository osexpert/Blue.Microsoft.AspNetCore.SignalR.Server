namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IJavaScriptProxyGenerator
	{
		string GenerateProxy(string serviceUrl);
	}
}
