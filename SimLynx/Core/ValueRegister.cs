
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SimLynx.Core;

/// <summary>
/// A type of <see cref="Register"/> that specifically registers a type of value.
/// </summary>
/// <remarks>
/// Behaves similarly to a dictionary, but adds are always additive and values cannot be removed or overridden.
/// <br/>
/// Values are always enumerate in registration order.
/// </remarks>
public abstract class ValueRegister<TKey, TValue>
    : Register, IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{

    /// <summary>
    /// Event raised when a new value is registered.
    /// </summary>
    public event Action<TValue>? OnRegistered;

    private readonly Dictionary<TKey, TValue> dictionary = [];
    private readonly List<TKey> registrations = [];

    /// <inheritdoc />
    public IEnumerable<TKey> Keys => dictionary.Keys;

    /// <inheritdoc/>
    public IEnumerable<TValue> Values => dictionary.Values;

    /// <inheritdoc/>
    public int Count => dictionary.Count;

    /// <inheritdoc/>
    public TValue this[TKey key] => dictionary[key];

    /// <inheritdoc/>
    public bool ContainsKey(TKey key)
        => dictionary.ContainsKey(key);

    /// <inheritdoc/>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        => dictionary.TryGetValue(key, out value);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        => registrations
            .Select(key => new KeyValuePair<TKey, TValue>(key, dictionary[key]))
            .GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Tries to register a value. Returns true if the registration was successful, false if a value with the same key
    /// already exists in the register.
    /// </summary>
    /// <param name="value">The value to register.</param>
    /// <returns>True if the registration was successful, false if a value with the same key already exists.</returns>
    public virtual bool TryRegisterValue(TValue value)
    {
        var key = GetRegistrationKey(value);
        var result = dictionary.TryAdd(key, value);

        if (result)
        {
            registrations.Add(key);
            OnRegistered?.Invoke(value);
        }

        return result;
    }

    /// <summary>
    /// Gets the registration key for a given value. This is used to determine how to register the value in the
    /// container.
    /// </summary>
    /// <param name="value">The value for which to get the registration key.</param>
    /// <returns>The registration key for the given value.</returns>
    protected abstract TKey GetRegistrationKey(TValue value);
}
