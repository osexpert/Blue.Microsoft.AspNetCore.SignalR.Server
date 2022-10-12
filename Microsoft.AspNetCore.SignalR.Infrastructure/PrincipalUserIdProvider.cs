using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public class PrincipalUserIdProvider : IUserIdProvider
	{
		public string GetUserId(HttpRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}
			ClaimsPrincipal user = request.get_HttpContext().get_User();
			if (user != null && user.Identity != null)
			{
				return user.Identity.Name;
			}
			return null;
		}
	}
}
