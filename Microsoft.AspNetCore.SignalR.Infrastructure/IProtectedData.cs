namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public interface IProtectedData
	{
		string Protect(string data, string purpose);

		string Unprotect(string protectedValue, string purpose);
	}
}
