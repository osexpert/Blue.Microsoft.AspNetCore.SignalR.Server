using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal class SafeSet<T>
	{
		private readonly ConcurrentDictionary<T, object> _items;

		public long Count => _items.Count;

		public SafeSet()
		{
			_items = new ConcurrentDictionary<T, object>();
		}

		public SafeSet(IEqualityComparer<T> comparer)
		{
			_items = new ConcurrentDictionary<T, object>(comparer);
		}

		public SafeSet(IEnumerable<T> items)
		{
			_items = new ConcurrentDictionary<T, object>(items.Select((T x) => new KeyValuePair<T, object>(x, null)));
		}

		public ICollection<T> GetSnapshot()
		{
			return _items.Keys;
		}

		public bool Contains(T item)
		{
			return _items.ContainsKey(item);
		}

		public bool Add(T item)
		{
			return _items.TryAdd(item, null);
		}

		public bool Remove(T item)
		{
			object value;
			return _items.TryRemove(item, out value);
		}

		public bool Any()
		{
			return _items.Any();
		}
	}
}
