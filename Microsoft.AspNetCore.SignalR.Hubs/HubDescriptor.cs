using System;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class HubDescriptor : Descriptor
	{
		public virtual Type HubType
		{
			get;
			set;
		}

		public string CreateQualifiedName(string unqualifiedName)
		{
			return Name + "." + unqualifiedName;
		}
	}
}
