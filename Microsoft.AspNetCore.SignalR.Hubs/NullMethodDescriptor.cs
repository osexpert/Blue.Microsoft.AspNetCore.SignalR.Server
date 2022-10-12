using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class NullMethodDescriptor : MethodDescriptor
	{
		private static readonly IEnumerable<Attribute> _attributes = new List<Attribute>();

		private static readonly IList<ParameterDescriptor> _parameters = new List<ParameterDescriptor>();

		private readonly string _methodName;

		private readonly IEnumerable<MethodDescriptor> _availableMethods;

		public override Func<IHub, object[], object> Invoker => delegate
		{
			IEnumerable<string> enumerable = GetAvailableMethodSignatures().ToArray();
			string format = (enumerable.Any() ? string.Format(CultureInfo.CurrentCulture, Resources.Error_MethodCouldNotBeResolvedCandidates, _methodName, "\n" + string.Join("\n", enumerable)) : string.Format(CultureInfo.CurrentCulture, Resources.Error_MethodCouldNotBeResolved, _methodName));
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, format));
		};

		public override IList<ParameterDescriptor> Parameters => _parameters;

		public override IEnumerable<Attribute> Attributes => _attributes;

		public NullMethodDescriptor(HubDescriptor descriptor, string methodName, IEnumerable<MethodDescriptor> availableMethods)
		{
			_methodName = methodName;
			_availableMethods = availableMethods;
			Hub = descriptor;
		}

		private IEnumerable<string> GetAvailableMethodSignatures()
		{
			return _availableMethods.Select((MethodDescriptor m) => m.Name + "(" + string.Join(", ", m.Parameters.Select((ParameterDescriptor p) => p.Name + ":" + p.ParameterType.Name)) + "):" + m.ReturnType.Name);
		}
	}
}
