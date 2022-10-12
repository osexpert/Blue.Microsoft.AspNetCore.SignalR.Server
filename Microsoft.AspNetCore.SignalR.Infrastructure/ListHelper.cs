using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal class ListHelper<T>
	{
		public static readonly IList<T> Empty = new ReadOnlyCollection<T>(new List<T>());
	}
}
