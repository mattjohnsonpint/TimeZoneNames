using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace TimeZoneNames;

internal class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    where TKey : notnull
{
    // TODO: Find a better implementation.
    // We might only need an IReadOnlyDictionary<TKey, TValue> but we want to preserve insertion order.
    private readonly OrderedDictionary _dictionary;

    public OrderedDictionary(int capacity = 0, IEqualityComparer<TKey>? equalityComparer = default)
    {
        _dictionary = new OrderedDictionary(capacity, (IEqualityComparer?) equalityComparer);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary
        .Cast<DictionaryEntry>()
        .Select(x => new KeyValuePair<TKey, TValue>((TKey) x.Key, (TValue) x.Value!))
        .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(KeyValuePair<TKey, TValue> item) => _dictionary.Add(item.Key, item.Value);

    public void Clear() => _dictionary.Clear();

    public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary[item.Key]?.Equals(item.Value) == true;

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        var i = 0;
        foreach (var entry in _dictionary.Cast<DictionaryEntry>())
        {
            if (i >= arrayIndex && i < array.Length)
            {
                array[i] = new KeyValuePair<TKey, TValue>((TKey) entry.Key, (TValue) entry.Value!);
            }
        
            i++;
        }
    }

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

#pragma warning disable CS8767
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
#pragma warning restore CS8767
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
        get => _dictionary[key] is { } value ? (TValue) value : throw new KeyNotFoundException();
        set => _dictionary[key] = value;
    }

    ICollection<TValue> IDictionary<TKey, TValue>.Values => _dictionary.Values.Cast<TValue>().ToList();

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => _dictionary.Keys.Cast<TKey>().ToList();
}

internal static class OrderedDictionaryExtensions
{
    public static OrderedDictionary<TKey, TValue> ToOrderedDictionary<TKey, TValue>(
        this ICollection<KeyValuePair<TKey, TValue>> items,
        IEqualityComparer<TKey>? comparer = default)
        where TKey : notnull
    {
        var result = new OrderedDictionary<TKey, TValue>(items.Count, comparer);
        foreach (var item in items)
        {
            result.Add(item);
        }

        return result;
    }

    public static OrderedDictionary<TKey, TValue> ToOrderedDictionary<TSource, TKey, TValue>(
        this ICollection<TSource> items,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector,
        IEqualityComparer<TKey>? comparer = default)
        where TKey : notnull
    {
        var result = new OrderedDictionary<TKey, TValue>(items.Count, comparer);
        foreach (var item in items)
        {
            var key = keySelector(item);
            var value = valueSelector(item);
            result.Add(key, value);
        }

        return result;
    }

    public static OrderedDictionary<TKey, TValue> ToOrderedDictionary<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> items,
        IEqualityComparer<TKey>? comparer = default)
        where TKey : notnull
        => items.ToList().ToOrderedDictionary(comparer);

    public static OrderedDictionary<TKey, TValue> ToOrderedDictionary<TSource, TKey, TValue>(
        this IEnumerable<TSource> items,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector,
        IEqualityComparer<TKey>? comparer = default)
        where TKey : notnull
        => items.ToList().ToOrderedDictionary(keySelector, valueSelector, comparer);
}