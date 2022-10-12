using System;
using System.Dynamic;
using System.Globalization;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	internal class NullClientProxy : DynamicObject
	{
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_UsingHubInstanceNotCreatedUnsupported));
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_UsingHubInstanceNotCreatedUnsupported));
		}
	}
}
