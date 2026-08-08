using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SimLynx;

/// <summary>
/// Dictionary which maps a strong type to a value.
/// </summary>
/// <remarks>
/// Only the exact type (including generics) is used for storage/lookup, not any base types or interfaces.
/// </remarks>
public class TypeDictionary : IReadOnlyDictionary<TypeKey, object>, ICloneable<TypeDictionary>
{
    private readonly Dictionary<TypeKey, object> _dictionary = [];

    /// <inheritdoc/>
    public object this[TypeKey key] => _dictionary[key];

    /// <inheritdoc/>
    public IEnumerable<TypeKey> Keys => _dictionary.Keys;

    /// <inheritdoc/>
    public IEnumerable<object> Values => _dictionary.Values;

    /// <inheritdoc/>
    public int Count => _dictionary.Count;

    /// <inheritdoc/>
    public bool ContainsKey(TypeKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    /// <summary>
    /// Gets whether this dictionary contains a <typeparamref name="T"/> value.
    /// </summary>
    /// <typeparam name="T">The type to check for in the dictionary.</typeparam>
    /// <returns>True if the dictionary contains a value for the type <typeparamref name="T"/>, otherwise false.</returns>
    public bool ContainsKey<T>()
    {
        return _dictionary.ContainsKey(new TypeKey(typeof(T)));
    }

    /// <summary>
    /// Gets whether this dictionary contains a <typeparamref name="T"/> value with the specified <paramref name="tag"/>.
    /// </summary>
    /// <typeparam name="T">The type to check for in the dictionary.</typeparam>
    /// <param name="tag">The tag associated with the type.</param>
    /// <returns>True if the dictionary contains a value for the type <typeparamref name="T"/> with the specified tag, otherwise false.</returns>
    public bool ContainsKey<T>(Symbol tag)
    {
        return _dictionary.ContainsKey(new TypeKey(typeof(T), tag));
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TypeKey, object>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

#pragma warning disable CS8767 // IReadOnlyDictionary lacks nullability annotations, so this ends up throwing a warning
    /// <inheritdoc/>
    public bool TryGetValue(TypeKey key, [MaybeNullWhen(false)] out object value)
    {
        return _dictionary.TryGetValue(key, out value);
    }
#pragma warning restore CS8767

    /// <summary>
    /// Gets a <typeparamref name="T"/> value from the dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the value to get from the dictionary.</typeparam>
    /// <param name="value">When this method returns, contains the value associated with the specified type, if the type is found; otherwise, the default value for the type.</param>
    /// <returns>True if the dictionary contains a value for the specified type; otherwise, false.</returns>
    public bool TryGet<T>([MaybeNullWhen(false)] out T value)
    {
        return TryGet<T>(Symbol.Empty, out value);
    }

    /// <summary>
    /// Gets a <typeparamref name="T"/> value from the dictionary with the specified <paramref name="tag"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to get from the dictionary.</typeparam>
    /// <param name="tag">The tag associated with the type.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified type and tag, if the type is found; otherwise, the default value for the type.</param>
    /// <returns>True if the dictionary contains a value for the specified type and tag; otherwise, false.</returns>
    public bool TryGet<T>(Symbol tag, [MaybeNullWhen(false)] out T value)
    {
        if (_dictionary.TryGetValue(new TypeKey(typeof(T), tag), out var obj))
        {
            value = (T)obj;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Gets a <typeparamref name="T"/> value from the dictionary, or adds a new value using the provided factory if it
    /// does not exist.
    /// <br/>
    /// Returns <c>true</c> if the value was added, <c>false</c> if it already existed. In either case,
    /// <paramref name="value"/> will contain the value from the dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the value to get or add in the dictionary.</typeparam>
    /// <param name="tag">The tag associated with the type.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified type and tag, if the type is found; otherwise, a new instance of the type.</param>
    /// <param name="factory">The factory function to create a new value if it does not exist.</param>
    /// <returns>True if the value was added, false if it already existed.</returns>
    public bool GetOrAdd<T>(Symbol tag, out T value, Func<T> factory)
    {
        if (!TryGet(tag, out value!))
        {
            value = factory();
            _dictionary[new TypeKey(typeof(T), tag)] = value!;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets a <typeparamref name="T"/> value from the dictionary, or adds a new value using the provided factory if
    /// it does not exist.
    /// <br/>
    /// Returns <c>true</c> if the value was added, <c>false</c> if it already existed. In either case,
    /// <paramref name="value"/> will contain the value from the dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the value to get or add in the dictionary.</typeparam>
    /// <param name="value">When this method returns, contains the value associated with the specified type, if the type is found; otherwise, a new instance of the type.</param>
    /// <param name="factory">The factory function to create a new value if it does not exist.</param>
    /// <returns>True if the value was added, false if it already existed.</returns>
    public bool GetOrAdd<T>(out T value, Func<T> factory)
    {
        if (!TryGet(out value!))
        {
            value = factory();
            _dictionary[typeof(T)] = value!;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets a <typeparamref name="T"/> value from the dictionary, or adds a new value using the default constructor if
    /// it does not exist.
    /// <br/>
    /// Returns <c>true</c> if the value was added, <c>false</c> if it already existed. In either case,
    /// <paramref name="value"/> will contain the value from the dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the value to get or add in the dictionary.</typeparam>
    /// <param name="tag">The tag associated with the type.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified type and tag, if
    /// the type is found; otherwise, a new instance of the type.</param>
    /// <returns>True if the value was added, false if it already existed.</returns>
    public bool GetOrAdd<T>(Symbol tag, out T value)
        where T : new()
    {
        return GetOrAdd(tag, out value, () => new T());
    }

    /// <summary>
    /// Gets a <typeparamref name="T"/> value from the dictionary, or adds a new value using the default constructor if
    /// it does not exist.
    /// <br/>
    /// Returns <c>true</c> if the value was added, <c>false</c> if it already existed. In either case,
    /// <paramref name="value"/> will contain the value from the dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the value to get or add in the dictionary.</typeparam>
    /// <param name="value">When this method returns, contains the value associated with the specified type, if the type is found; otherwise, a new instance of the type.</param>
    /// <returns>True if the value was added, false if it already existed.</returns>
    public bool GetOrAdd<T>(out T value)
        where T : new()
    {
        return GetOrAdd(out value, () => new T());
    }

    /// <summary>
    /// Gets a <typeparamref name="T"/> value from the dictionary, or adds a new value using the provided factory if it
    /// does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the value to get or add in the dictionary.</typeparam>
    /// <param name="tag">The tag associated with the type.</param>
    /// <param name="factory">The factory function to create a new value if it does not exist.</param>
    /// <returns>The existing or newly added value of type <typeparamref name="T"/>.</returns>
    public T GetOrAdd<T>(Symbol tag, Func<T> factory)
    {
        GetOrAdd(tag, out T value, factory);
        return value;
    }

    /// <summary>
    /// Gets a <typeparamref name="T"/> value from the dictionary, or adds a new value using the provided factory if
    /// it does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the value to get or add in the dictionary.</typeparam>
    /// <param name="factory">The factory function to create a new value if it does not exist.</param>
    /// <returns>The existing or newly added value of type <typeparamref name="T"/>.</returns>
    public T GetOrAdd<T>(Func<T> factory)
    {
        GetOrAdd(out T value, factory);
        return value;
    }

    /// <summary>
    /// Gets a <typeparamref name="T"/> value from the dictionary, or adds a new value using the default constructor
    /// if it does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the value to get or add in the dictionary.</typeparam>
    /// <param name="tag">The tag associated with the type.</param>
    /// <returns>The existing or newly added value of type <typeparamref name="T"/>.</returns>
    public T GetOrAdd<T>(Symbol tag)
        where T : new()
    {
        return GetOrAdd(tag, () => new T());
    }

    /// <summary>
    /// Gets a <typeparamref name="T"/> value from the dictionary, or adds a new value using the default constructor if
    /// it does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the value to get or add in the dictionary.</typeparam>
    /// <returns>The existing or newly added value of type <typeparamref name="T"/>.</returns>
    public T GetOrAdd<T>()
        where T : new()
    {
        return GetOrAdd(() => new T());
    }

    /// <summary>
    /// Adds or updates a value in the dictionary for the specified type <typeparamref name="T"/> and
    /// <paramref name="tag"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to add or update in the dictionary.</typeparam>
    /// <param name="tag">The tag associated with the type.</param>
    /// <param name="value">The value to add or update in the dictionary.</param>
    /// <returns>The value that was added or updated.</returns>
    public T Put<T>(Symbol tag, T value)
    {
        _dictionary[new TypeKey(typeof(T), tag)] = value!;
        return value;
    }

    /// <summary>
    /// Adds or updates a value in the dictionary for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to add or update in the dictionary.</typeparam>
    /// <param name="value">The value to add or update in the dictionary.</param>
    /// <returns>The value that was added or updated.</returns>
    public T Put<T>(T value)
    {
        _dictionary[typeof(T)] = value!;
        return value;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    public TypeDictionary Clone()
    {
        var clone = new TypeDictionary();
        foreach (var kvp in _dictionary)
        {
            clone._dictionary[kvp.Key] = kvp.Value;
        }
        return clone;
    }
}
