using System;
using Microsoft.AspNetCore.Http;

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
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}
			if (Version.TryParse(request.Query["clientProtocol"], out var result))
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
