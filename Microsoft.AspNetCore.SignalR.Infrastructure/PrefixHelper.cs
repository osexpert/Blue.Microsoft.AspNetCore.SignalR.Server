using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal static class PrefixHelper
	{
		internal const string HubPrefix = "h-";

		internal const string HubGroupPrefix = "hg-";

		internal const string HubConnectionIdPrefix = "hc-";

		internal const string HubUserPrefix = "hu-";

		internal const string PersistentConnectionPrefix = "pc-";

		internal const string PersistentConnectionGroupPrefix = "pcg-";

		internal const string ConnectionIdPrefix = "c-";

		public static bool HasGroupPrefix(string value)
		{
			if (!value.StartsWith("hg-", StringComparison.Ordinal))
			{
				return value.StartsWith("pcg-", StringComparison.Ordinal);
			}
			return true;
		}

		public static string GetConnectionId(string connectionId)
		{
			return "c-" + connectionId;
		}

		public static string GetHubConnectionId(string connectionId)
		{
			return "hc-" + connectionId;
		}

		public static string GetHubName(string connectionId)
		{
			return "h-" + connectionId;
		}

		public static string GetHubGroupName(string groupName)
		{
			return "hg-" + groupName;
		}

		public static string GetHubUserId(string userId)
		{
			return "hu-" + userId;
		}

		public static string GetPersistentConnectionGroupName(string groupName)
		{
			return "pcg-" + groupName;
		}

		public static string GetPersistentConnectionName(string connectionName)
		{
			return "pc-" + connectionName;
		}

		public static IList<string> GetPrefixedConnectionIds(IList<string> connectionIds)
		{
			if (connectionIds.Count == 0)
			{
				return ListHelper<string>.Empty;
			}
			return connectionIds.Select(GetConnectionId).ToList();
		}

		public static IEnumerable<string> RemoveGroupPrefixes(IEnumerable<string> groups)
		{
			return groups.Select(RemoveGroupPrefix);
		}

		public static string RemoveGroupPrefix(string name)
		{
			if (name.StartsWith("hg-", StringComparison.Ordinal))
			{
				return name.Substring("hg-".Length);
			}
			if (name.StartsWith("pcg-", StringComparison.Ordinal))
			{
				return name.Substring("pcg-".Length);
			}
			return name;
		}
	}
}
