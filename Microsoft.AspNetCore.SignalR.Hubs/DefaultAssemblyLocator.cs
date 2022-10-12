using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyModel;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class DefaultAssemblyLocator : IAssemblyLocator
	{
		private static readonly string AssemblyRoot = typeof(Hub).GetTypeInfo().Assembly.GetName()
			.Name;

		private readonly Assembly _entryAssembly;

		private readonly DependencyContext _dependencyContext;

		public DefaultAssemblyLocator(IHostingEnvironment environment)
		{
			_entryAssembly = Assembly.Load(new AssemblyName(environment.ApplicationName));
			_dependencyContext = DependencyContext.Load(_entryAssembly);
		}

		public virtual IList<Assembly> GetAssemblies()
		{
			if (_dependencyContext == null)
			{
				return new Assembly[1]
				{
					_entryAssembly
				};
			}
			return (from assembly in _dependencyContext.RuntimeLibraries.Where(IsCandidateLibrary).SelectMany((RuntimeLibrary l) => l.GetDefaultAssemblyNames(_dependencyContext))
				select Assembly.Load(new AssemblyName(assembly.Name))).ToArray();
		}

		private bool IsCandidateLibrary(RuntimeLibrary library)
		{
			return library.Dependencies.Any((Dependency dependency) => string.Equals(AssemblyRoot, dependency.Name, StringComparison.Ordinal));
		}
	}
}
