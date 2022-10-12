using System;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class HubNameAttribute : Attribute
	{
		public string HubName
		{
			get;
			private set;
		}

		public HubNameAttribute(string hubName)
		{
			if (string.IsNullOrEmpty(hubName))
			{
				throw new ArgumentNullException("hubName");
			}
			HubName = hubName;
		}
	}
}
