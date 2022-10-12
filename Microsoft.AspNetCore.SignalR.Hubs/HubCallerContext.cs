using System.Security.Principal;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class HubCallerContext
	{
		public virtual string ConnectionId
		{
			get;
			private set;
		}

		public virtual IRequestCookieCollection RequestCookies => Request.Cookies;

		public virtual IHeaderDictionary Headers => Request.Headers;

		public virtual IQueryCollection QueryString => Request.Query;

		public virtual IPrincipal User => Request.HttpContext.User;

		public virtual HttpRequest Request
		{
			get;
			private set;
		}

		protected HubCallerContext()
		{
		}

		public HubCallerContext(HttpRequest request, string connectionId)
		{
			ConnectionId = connectionId;
			Request = request;
		}
	}
}
