using System.Globalization;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class EmptyJavaScriptProxyGenerator : IJavaScriptProxyGenerator
	{
		public string GenerateProxy(string serviceUrl)
		{
			return string.Format(CultureInfo.InvariantCulture, "throw new Error('{0}');", Resources.Error_JavaScriptProxyDisabled);
		}
	}
}
