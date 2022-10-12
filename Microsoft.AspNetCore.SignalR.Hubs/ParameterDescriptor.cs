using System;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class ParameterDescriptor
	{
		public virtual string Name
		{
			get;
			set;
		}

		public virtual Type ParameterType
		{
			get;
			set;
		}
	}
}
