using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SignalR
{
	internal static class RequestExtensions
	{
		public static string LocalPath(this HttpRequest request)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			PathString val = request.get_PathBase();
			string value = val.get_Value();
			val = request.get_Path();
			return value + val.get_Value();
		}
	}
}
