using System;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class HubMethodNameAttribute : Attribute
	{
		public string MethodName
		{
			get;
			private set;
		}

		public HubMethodNameAttribute(string methodName)
		{
			if (string.IsNullOrEmpty(methodName))
			{
				throw new ArgumentNullException("methodName");
			}
			MethodName = methodName;
		}
	}
}
