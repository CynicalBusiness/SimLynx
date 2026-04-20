using System.Collections;
using System.Collections.Generic;

namespace SimLynx.Core;

/// <summary>
/// A set that works much the same as a standard <see cref="HashSet{T}"/>, but guarantees items are maintained in
/// insertion order.
/// </summary>
/// <remarks>
/// If a duplicate is added, the new entry is ignored. To move an item to a different location in this collection,
/// remove and re-add it.
/// </remarks>
/// <typeparam name="T">The type of elements in the set.</typeparam>
/// <param name="comparer">The equality comparer to use for the set.</param>
public class LinkedSet<T>(IEqualityComparer<T> comparer) : ICollection<T>
{
    private readonly Dictionary<T, LinkedListNode<T>> _index = new(comparer);
    private readonly LinkedList<T> _list = [];

    /// <summary>
    /// Creates a new linked set with the default equality comparer.
    /// </summary>
    public LinkedSet()
        : this(EqualityComparer<T>.Default) { }

    /// <inheritdoc/>
    public int Count => _list.Count;

    /// <inheritdoc/>
    public bool IsReadOnly { get; } = false;

    /// <inheritdoc/>
    public bool Add(T item)
    {
        if (_index.ContainsKey(item))
        {
            return false;
        }

        _index[item] = _list.AddLast(item);
        return true;
    }

    void ICollection<T>.Add(T item) => Add(item);

    /// <inheritdoc/>
    public void Clear()
    {
        _index.Clear();
        _list.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(T item)
    {
        return _index.ContainsKey(item);
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public bool Remove(T item)
    {
        if (!_index.TryGetValue(item, out var node))
        {
            return false;
        }

        _index.Remove(item);
        _list.Remove(node);
        return true;
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }
}
