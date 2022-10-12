using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class MethodDescriptor : Descriptor
	{
		public virtual Type ReturnType
		{
			get;
			set;
		}

		public virtual HubDescriptor Hub
		{
			get;
			set;
		}

		public virtual IList<ParameterDescriptor> Parameters
		{
			get;
			set;
		}

		public Type ProgressReportingType
		{
			get;
			set;
		}

		public virtual Func<IHub, object[], object> Invoker
		{
			get;
			set;
		}

		public virtual IEnumerable<Attribute> Attributes
		{
			get;
			set;
		}
	}
}
