using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace TimeZoneNames;

internal class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    // TODO: Find a better implementation.
    // We might only need an IReadOnlyDictionary<TKey, TValue> but we want to preserve insertion order.
    private readonly OrderedDictionary _dictionary;

    public OrderedDictionary(int capacity, IEqualityComparer<TKey> equalityComparer)
    {
        _dictionary = new OrderedDictionary(capacity, (IEqualityComparer) equalityComparer);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary
        .Cast<DictionaryEntry>()
        .Select(x => new KeyValuePair<TKey, TValue>((TKey) x.Key, (TValue) x.Value))
        .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(KeyValuePair<TKey, TValue> item) => _dictionary.Add(item.Key, item.Value);

    public void Clear() => _dictionary.Clear();

    public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary[item.Key]?.Equals(item.Value) == true;

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _dictionary.CopyTo(array, arrayIndex);

    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

    public int Count => _dictionary.Count;

    public bool IsReadOnly => _dictionary.IsReadOnly;

    public void Add(TKey key, TValue value) => _dictionary.Add(key, value);

    public bool ContainsKey(TKey key) => _dictionary.Contains(key);
    
    public bool Remove(TKey key)
    {
        var found = _dictionary.Contains(key);
        if (found)
        {
            _dictionary.Remove(key);
        }

        return found;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        var obj = _dictionary[key];
        if (obj == null)
        {
            value = default;
            return false;
        }
        
        value = (TValue) obj;
        return true;
    }

    public TValue this[TKey key]
    {
        get => (TValue) _dictionary[key];
        set => _dictionary[key] = value;
    }

    ICollection<TValue> IDictionary<TKey, TValue>.Values => _dictionary.Values.Cast<TValue>().ToList();

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => _dictionary.Keys.Cast<TKey>().ToList();
}