using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal struct DiffPair<T>
	{
		public ICollection<T> Added;

		public ICollection<T> Removed;

		public bool AnyChanges
		{
			get
			{
				if (Added.Count <= 0)
				{
					return Removed.Count > 0;
				}
				return true;
			}
		}
	}
}
