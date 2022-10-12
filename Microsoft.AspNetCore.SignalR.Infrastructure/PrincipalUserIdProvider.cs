using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

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
			ClaimsPrincipal user = request.HttpContext.User;
			if (user != null && user.Identity != null)
			{
				return user.Identity.Name;
			}
			return null;
		}
	}
}
