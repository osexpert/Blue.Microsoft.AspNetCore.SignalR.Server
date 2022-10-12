using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IAssemblyLocator
	{
		IList<Assembly> GetAssemblies();
	}
}
