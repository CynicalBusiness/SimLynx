using System;

namespace SimLynx;

/// <summary>
/// A dictionary that maps values by their type and optional tag.
/// </summary>
/// <typeparam name="TLimit">The upper bound type for the values stored in the dictionary.</typeparam>
public interface ITypeDictionary<TLimit> : IReadOnlyTypeDictionary<TLimit>
{
    /// <summary>
    /// Sets the value of type <typeparamref name="T"/> associated with the specified <paramref name="id"/>.
    /// </summary>
    /// <remarks>
    /// If a <typeparamref name="T"/> value already exists for the tag, it is replaced; otherwise, a new entry is added.
    /// </remarks>
    /// <typeparam name="T">The type of the value to set in the dictionary.</typeparam>
    /// <param name="id">The optional ID associated with the value.</param>
    /// <param name="value">The value to set in the dictionary.</param>
    public void Set<T>(Identifier id, T value)
        where T : TLimit;

    /// <summary>
    /// Gets the value associated with the specified <paramref name="key"/>. If the value does not exist, it is created
    /// using the provided <paramref name="valueFactory"/> and added to the dictionary.
    /// </summary>
    /// <param name="key">The key associated with the value.</param>
    /// <param name="valueFactory">A function that creates the value if it does not exist.</param>
    /// <returns>The existing or newly created value.</returns>
    public TLimit GetOrAdd(TypeKey key, Func<TLimit> valueFactory);

    /// <summary>
    /// Removes the value associated with the specified <paramref name="key"/>, if it exists.
    /// </summary>
    /// <param name="key">The key associated with the value to remove.</param>
    /// <returns>True if the value was removed, otherwise false.</returns>
    public bool Remove(TypeKey key);

    /// <summary>
    /// Removes the value of type <typeparamref name="T"/> associated with the specified <paramref name="tag"/>, if it
    /// exists.
    /// </summary>
    /// <typeparam name="T">The type of the value to remove.</typeparam>
    /// <param name="tag">The optional tag associated with the value.</param>
    /// <returns>True if the value was removed, otherwise false.</returns>
    public bool Remove<T>(Symbol tag)
        where T : TLimit;
}
