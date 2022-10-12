using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public class ProtocolResolver
	{
		private const string ProtocolQueryParameter = "clientProtocol";

		private readonly Version _minSupportedProtocol;

		private readonly Version _maxSupportedProtocol;

		private readonly Version _minimumDelayedStartVersion = new Version(1, 4);

		public ProtocolResolver()
			: this(new Version(1, 2), new Version(1, 5))
		{
		}

		public ProtocolResolver(Version min, Version max)
		{
			_minSupportedProtocol = min;
			_maxSupportedProtocol = max;
		}

		public Version Resolve(HttpRequest request)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}
			if (Version.TryParse(StringValues.op_Implicit(request.get_Query().get_Item("clientProtocol")), out Version result))
			{
				if (result > _maxSupportedProtocol)
				{
					result = _maxSupportedProtocol;
				}
				else if (result < _minSupportedProtocol)
				{
					result = _minSupportedProtocol;
				}
			}
			return result ?? _minSupportedProtocol;
		}

		public bool SupportsDelayedStart(HttpRequest request)
		{
			return Resolve(request) >= _minimumDelayedStartVersion;
		}
	}
}
