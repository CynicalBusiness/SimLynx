using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimLynx;

/// <summary>
/// A set that maintains insertion order <em>and</em> uniqueness of its elements.
/// </summary>
/// <remarks>
/// For the purposes of this set, "insertion order" means the order in which the <em>first</em> instance of each element
/// that was added to the set.
/// </remarks>
/// <typeparam name="T">The type of elements in the set.</typeparam>
public class LinkedHashSet<T>(IEqualityComparer<T> comparer) : ISet<T>
    where T : notnull
{
    private Node? _head;
    private Node? _tail;
    private readonly Dictionary<T, Node> _map = new(comparer);

    /// <summary>
    /// Creates a new set class that uses the default equality comparer for the set type.
    /// </summary>
    public LinkedHashSet()
        : this(EqualityComparer<T>.Default) { }

    /// <summary>
    /// Creates a new set that contains elements copied from the specified <paramref name="collection"/> and uses the
    /// default equality comparer for the set type.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    public LinkedHashSet(IEnumerable<T> collection)
        : this(collection, EqualityComparer<T>.Default)
    {
        AddRange(collection);
    }

    /// <summary>
    /// Creates a new set that contains elements copied from the specified <paramref name="collection"/> and uses the
    /// specified equality <paramref name="comparer"/> for the set type.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    /// <param name="comparer">The equality comparer to use for the set.</param>
    public LinkedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        : this(comparer)
    {
        AddRange(collection);
    }

    /// <summary>
    /// The comparer used to determine equality of elements in the set.
    /// </summary>
    public IEqualityComparer<T> Comparer => _map.Comparer;

    /// <inheritdoc/>
    public int Count => _map.Count;

    /// <inheritdoc/>
    public bool Add(T item)
    {
        var node = new Node(item);
        if (_map.TryAdd(item, node))
        {
            if (_tail is null)
            {
                _head = _tail = node;
            }
            else
            {
                node.Previous = _tail;
                _tail.Next = node;
                _tail = node;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of this set.
    /// </summary>
    /// <param name="items">The items to add</param>
    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of this set.
    /// </summary>
    /// <param name="items">The items to add</param>
    public void AddRange(ReadOnlySpan<T> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _head = _tail = null;
        _map.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(T item)
    {
        return _map.ContainsKey(item);
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        AddRange(array.AsSpan(arrayIndex));
    }

    /// <inheritdoc/>
    public void ExceptWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            Remove(item);
        }
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        var current = _head;
        while (current is not null)
        {
            yield return current.Value;
            current = current.Next;
        }
    }

    /// <inheritdoc/>
    public void IntersectWith(IEnumerable<T> other)
    {
        var toRemove = new HashSet<T>(other, Comparer);

        foreach (var item in other)
        {
            toRemove.Remove(item);
        }

        foreach (var item in toRemove)
        {
            Remove(item);
        }
    }

    /// <inheritdoc/>
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        var set = new HashSet<T>(other, Comparer);
        return set.IsProperSupersetOf(this);
    }

    /// <inheritdoc/>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        var count = 0;
        foreach (var item in other)
        {
            if (!Contains(item))
            {
                return false;
            }
            count++;
        }
        return count < Count;
    }

    /// <inheritdoc/>
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        var set = new HashSet<T>(this, Comparer);
        return set.IsSubsetOf(other);
    }

    /// <inheritdoc/>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        return other.All(Contains);
    }

    /// <inheritdoc/>
    public bool Overlaps(IEnumerable<T> other)
    {
        return other.Any(Contains);
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
        if (!_map.TryGetValue(item, out var node))
        {
            return false;
        }

        _map.Remove(item);
        if (node.Previous is not null)
        {
            node.Previous.Next = node.Next;
        }
        else
        {
            _head = node.Next;
        }

        if (node.Next is not null)
        {
            node.Next.Previous = node.Previous;
        }
        else
        {
            _tail = node.Previous;
        }

        return true;
    }

    /// <inheritdoc/>
    public bool SetEquals(IEnumerable<T> other)
    {
        var thisEn = GetEnumerator();
        var otherEn = other.GetEnumerator();

        while (thisEn.MoveNext())
        {
            if (!otherEn.MoveNext() || !Comparer.Equals(thisEn.Current, otherEn.Current))
            {
                return false;
            }
        }
        if (otherEn.MoveNext())
        {
            return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            if (!Add(item))
            {
                Remove(item);
            }
        }
    }

    /// <inheritdoc/>
    public void UnionWith(IEnumerable<T> other)
    {
        AddRange(other);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    bool ICollection<T>.IsReadOnly => false;

    void ICollection<T>.Add(T item) => Add(item);

    private record Node(T Value)
    {
        public Node? Previous { get; set; }
        public Node? Next { get; set; }
    }
}
