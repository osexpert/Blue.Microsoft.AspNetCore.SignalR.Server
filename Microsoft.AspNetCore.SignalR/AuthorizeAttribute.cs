using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Hubs;
using System;
using System.Linq;
using System.Security.Principal;

namespace Microsoft.AspNetCore.SignalR
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class AuthorizeAttribute : Attribute, IAuthorizeHubConnection, IAuthorizeHubMethodInvocation
	{
		private string _roles;

		private string[] _rolesSplit = new string[0];

		private string _users;

		private string[] _usersSplit = new string[0];

		protected bool? _requireOutgoing;

		public bool RequireOutgoing
		{
			get
			{
				throw new NotImplementedException(Resources.Error_DoNotReadRequireOutgoing);
			}
			set
			{
				_requireOutgoing = value;
			}
		}

		public string Roles
		{
			get
			{
				return _roles ?? string.Empty;
			}
			set
			{
				_roles = value;
				_rolesSplit = SplitString(value);
			}
		}

		public string Users
		{
			get
			{
				return _users ?? string.Empty;
			}
			set
			{
				_users = value;
				_usersSplit = SplitString(value);
			}
		}

		public virtual bool AuthorizeHubConnection(HubDescriptor hubDescriptor, HttpRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}
			if (_requireOutgoing.HasValue && !_requireOutgoing.Value)
			{
				return true;
			}
			return UserAuthorized(request.get_HttpContext().get_User());
		}

		public virtual bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
		{
			if (hubIncomingInvokerContext == null)
			{
				throw new ArgumentNullException("hubIncomingInvokerContext");
			}
			if (appliesToMethod && _requireOutgoing == true)
			{
				throw new ArgumentException(Resources.Error_MethodLevelOutgoingAuthorization);
			}
			return UserAuthorized(hubIncomingInvokerContext.Hub.Context.User);
		}

		protected virtual bool UserAuthorized(IPrincipal user)
		{
			if (user == null)
			{
				return false;
			}
			if (!user.Identity.IsAuthenticated)
			{
				return false;
			}
			if (_usersSplit.Length != 0 && !_usersSplit.Contains(user.Identity.Name, StringComparer.OrdinalIgnoreCase))
			{
				return false;
			}
			if (_rolesSplit.Length != 0 && !_rolesSplit.Any(user.IsInRole))
			{
				return false;
			}
			return true;
		}

		private static string[] SplitString(string original)
		{
			if (string.IsNullOrEmpty(original))
			{
				return new string[0];
			}
			return (from piece in original.Split(new char[1]
			{
				','
			})
			let trimmed = piece.Trim()
			where !string.IsNullOrEmpty(trimmed)
			select trimmed).ToArray();
		}
	}
}
