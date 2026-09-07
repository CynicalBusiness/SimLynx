using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SimLynx;

/// <summary>
/// Dictionary which maps a type to a value.
/// </summary>
/// <remarks>
/// Only the exact type (including generics) is used for storage/lookup, not any base types or interfaces.
/// </remarks>
/// <typeparam name="TLimit">The type to which the dictionary is limited.</typeparam>
/// <param name="comparer">The comparer to use for comparing <see cref="TypeKey"/> instances.</param>
public class TypeDictionary<TLimit>(IEqualityComparer<TypeKey>? comparer = null)
    : ITypeDictionary<TLimit>,
        ICloneable<TypeDictionary<TLimit>>
{
    internal readonly Dictionary<TypeKey, TLimit> _dictionary = new(comparer ?? TypeKey.Comparer.Default);

    /// <summary>
    /// Creates a new <see cref="TypeDictionary{T}"/> with the default comparer.
    /// </summary>
    public TypeDictionary()
        : this(comparer: null) { }

    /// <summary>
    /// Creates a new <see cref="TypeDictionary{T}"/> with the specified comparer, copying all entries from the
    /// specified <paramref name="source"/> dictionary.
    /// </summary>
    /// <param name="source">The source dictionary to copy entries from.</param>
    /// <param name="comparer">The comparer to use for comparing <see cref="TypeKey"/> instances.</param>
    public TypeDictionary(IReadOnlyTypeDictionary<TLimit> source, IEqualityComparer<TypeKey>? comparer = null)
        : this(comparer)
    {
        foreach (var kvp in source)
        {
            _dictionary[kvp.Key] = kvp.Value;
        }
    }

    /// <inheritdoc/>
    public TLimit this[TypeKey key]
    {
        get => _dictionary[key];
        set
        {
            if (!key.Type.IsAssignableTo(typeof(TLimit)))
            {
                throw new ArgumentException(
                    $"The type '{key.Type}' is not assignable to the dictionary limit type '{typeof(TLimit)}'.",
                    nameof(key)
                );
            }

            var valueType = value?.GetType();
            if (valueType is null)
            {
                // value types won't ever be null so we only enter this block for reference types
                _dictionary[key] = default!;
                return;
            }

            if (!valueType.IsAssignableTo(key.Type))
            {
                throw new ArgumentException(
                    $"The value of type '{valueType}' is not assignable to the key type '{key.Type}'.",
                    nameof(value)
                );
            }

            _dictionary[key] = value;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<TypeKey> Keys => _dictionary.Keys;

    /// <inheritdoc/>
    public IEnumerable<TLimit> Values => _dictionary.Values;

    /// <inheritdoc/>
    public int Count => _dictionary.Count;

    /// <summary>
    /// Gets the comparer used to compare <see cref="TypeKey"/> instances in this dictionary.
    /// </summary>
    public IEqualityComparer<TypeKey> Comparer => _dictionary.Comparer;

    /// <inheritdoc/>
    public bool ContainsKey(TypeKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    /// <inheritdoc/>
#pragma warning disable CS8767 // IReadOnlyDictionary has missing nullability attributes
    public bool TryGetValue(TypeKey key, [MaybeNullWhen(false)] out TLimit value)
#pragma warning restore CS8767
    {
        return _dictionary.TryGetValue(key, out value);
    }

    /// <inheritdoc/>
    public void Set<T>(Symbol tag, T value)
        where T : TLimit
    {
        _dictionary[new TypeKey(typeof(T), tag)] = value;
    }

    /// <inheritdoc/>
    public TLimit GetOrAdd(TypeKey key, Func<TLimit> valueFactory)
    {
        if (!_dictionary.TryGetValue(key, out var value))
        {
            value = valueFactory();
            _dictionary[key] = value;
        }
        return value;
    }

    /// <inheritdoc/>
    public bool Remove(TypeKey key)
    {
        return _dictionary.Remove(key);
    }

    /// <inheritdoc/>
    public bool Remove<T>(Symbol tag)
        where T : TLimit
    {
        return _dictionary.Remove(new TypeKey(typeof(T), tag));
    }

    /// <summary>
    /// Creates a readonly wrapper around this <see cref="TypeDictionary{TLimit}"/>.
    /// </summary>
    /// <remarks>
    /// Note that the readonly wrapper does not create a copy of the underlying dictionary, so changes to the original
    /// dictionary will be reflected in the readonly wrapper.
    /// </remarks>
    /// <returns>A readonly wrapper around this dictionary.</returns>
    public Readonly AsReadOnly()
    {
        return new Readonly(this);
    }

    /// <inheritdoc/>
    public TypeDictionary<TLimit> Clone()
    {
        var clone = new TypeDictionary<TLimit>(comparer);
        foreach (var kvp in _dictionary)
        {
            clone._dictionary[kvp.Key] = kvp.Value;
        }
        return clone;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TypeKey, TLimit>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Readonly wrapper around a <see cref="TypeDictionary{TLimit}"/>.
    /// </summary>
    /// <param name="source">The source dictionary.</param>
    public class Readonly(TypeDictionary<TLimit> source) : IReadOnlyTypeDictionary<TLimit>
    {
        /// <inheritdoc/>
        public TLimit this[TypeKey key] => source[key];

        /// <inheritdoc/>
        public IEnumerable<TypeKey> Keys => source.Keys;

        /// <inheritdoc/>
        public IEnumerable<TLimit> Values => source.Values;

        /// <inheritdoc/>
        public int Count => source.Count;

        /// <inheritdoc/>
        public bool ContainsKey(TypeKey key)
        {
            return source.ContainsKey(key);
        }

        /// <inheritdoc/>
#pragma warning disable CS8767 // IReadOnlyDictionary has missing nullability attributes
        public bool TryGetValue(TypeKey key, [MaybeNullWhen(false)] out TLimit value)
#pragma warning restore CS8767
        {
            return source.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TypeKey, TLimit>> GetEnumerator()
        {
            return source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
