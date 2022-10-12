using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class DynamicDictionary : DynamicObject, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
	{
		private readonly IDictionary<string, object> _obj;

		public object this[string key]
		{
			get
			{
				_obj.TryGetValue(key, out var value);
				return Wrap(value);
			}
			set
			{
				_obj[key] = Unwrap(value);
			}
		}

		public ICollection<string> Keys => _obj.Keys;

		public ICollection<object> Values => _obj.Values;

		public int Count => _obj.Count;

		public bool IsReadOnly => _obj.IsReadOnly;

		public DynamicDictionary(IDictionary<string, object> obj)
		{
			_obj = obj;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = this[binder.Name];
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			this[binder.Name] = value;
			return true;
		}

		public static object Wrap(object value)
		{
			IDictionary<string, object> dictionary = value as IDictionary<string, object>;
			if (dictionary != null)
			{
				return new DynamicDictionary(dictionary);
			}
			return value;
		}

		public static object Unwrap(object value)
		{
			DynamicDictionary dynamicDictionary = value as DynamicDictionary;
			if (dynamicDictionary != null)
			{
				return dynamicDictionary._obj;
			}
			return value;
		}

		public void Add(string key, object value)
		{
			_obj.Add(key, value);
		}

		public bool ContainsKey(string key)
		{
			return _obj.ContainsKey(key);
		}

		public bool Remove(string key)
		{
			return _obj.Remove(key);
		}

		public bool TryGetValue(string key, out object value)
		{
			return _obj.TryGetValue(key, out value);
		}

		public void Add(KeyValuePair<string, object> item)
		{
			_obj.Add(item);
		}

		public void Clear()
		{
			_obj.Clear();
		}

		public bool Contains(KeyValuePair<string, object> item)
		{
			return _obj.Contains(item);
		}

		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			_obj.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<string, object> item)
		{
			return _obj.Remove(item);
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return _obj.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
