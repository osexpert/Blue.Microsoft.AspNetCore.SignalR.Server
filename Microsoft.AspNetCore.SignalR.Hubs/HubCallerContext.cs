using Microsoft.AspNetCore.Http;
using System.Security.Principal;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class HubCallerContext
	{
		public virtual string ConnectionId
		{
			get;
			private set;
		}

		public virtual IRequestCookieCollection RequestCookies => Request.get_Cookies();

		public virtual IHeaderDictionary Headers => Request.get_Headers();

		public virtual IQueryCollection QueryString => Request.get_Query();

		public virtual IPrincipal User => Request.get_HttpContext().get_User();

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
