using System.Collections.Generic;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal class DiffSet<T>
	{
		private readonly HashSet<T> _items;

		private readonly HashSet<T> _addedItems;

		private readonly HashSet<T> _removedItems;

		public DiffSet(IEnumerable<T> items)
		{
			_addedItems = new HashSet<T>();
			_removedItems = new HashSet<T>();
			_items = new HashSet<T>(items);
		}

		public bool Add(T item)
		{
			if (_items.Add(item))
			{
				if (!_removedItems.Remove(item))
				{
					_addedItems.Add(item);
				}
				return true;
			}
			return false;
		}

		public bool Remove(T item)
		{
			if (_items.Remove(item))
			{
				if (!_addedItems.Remove(item))
				{
					_removedItems.Add(item);
				}
				return true;
			}
			return false;
		}

		public bool Contains(T item)
		{
			return _items.Contains(item);
		}

		public ICollection<T> GetSnapshot()
		{
			return _items;
		}

		public bool DetectChanges()
		{
			bool result = _addedItems.Count > 0 || _removedItems.Count > 0;
			_addedItems.Clear();
			_removedItems.Clear();
			return result;
		}
	}
}
