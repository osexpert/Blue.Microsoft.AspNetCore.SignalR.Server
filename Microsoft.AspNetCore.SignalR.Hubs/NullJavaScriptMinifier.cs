namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class NullJavaScriptMinifier : IJavaScriptMinifier
	{
		public string Minify(string source)
		{
			return source;
		}
	}
}
