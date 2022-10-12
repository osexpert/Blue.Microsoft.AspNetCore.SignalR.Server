using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class DefaultAssemblyLocator : IAssemblyLocator
	{
		private static readonly string AssemblyRoot = typeof(Hub).GetTypeInfo().get_Assembly().GetName()
			.Name;

			private readonly Assembly _entryAssembly;

			private readonly DependencyContext _dependencyContext;

			public DefaultAssemblyLocator(IHostingEnvironment environment)
			{
				_entryAssembly = Assembly.Load(new AssemblyName(environment.get_ApplicationName()));
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
				return (from assembly in _dependencyContext.get_RuntimeLibraries().Where(IsCandidateLibrary).SelectMany((RuntimeLibrary l) => DependencyContextExtensions.GetDefaultAssemblyNames(l, _dependencyContext))
				select Assembly.Load(new AssemblyName(assembly.Name))).ToArray();
			}

			private bool IsCandidateLibrary(RuntimeLibrary library)
			{
				return library.get_Dependencies().Any((Dependency dependency) => string.Equals(AssemblyRoot, dependency.get_Name(), StringComparison.Ordinal));
			}
		}
	}
