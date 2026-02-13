
using System;
using System.Collections.Generic;

namespace SimLynx;

/// <summary>
/// Utility extension methods for SimLynx.
/// </summary>
public static class SimLynxExtensions
{

    #region Symbols

    /// <summary>
    /// Converts an object to a symbol.
    /// </summary>
    /// <remarks>
    /// Essentially an alias for <c>Symbol.For(obj.ToString())</c>.
    /// </remarks>
    /// <param name="obj">The object to convert to a symbol.</param>
    /// <returns>A <see cref="Symbol"/> representing the object.</returns>
    public static Symbol ToSymbol(this object obj) => Symbol.For(
        obj.ToString() ?? throw new InvalidOperationException("Cannot convert null string to symbol."));

    #endregion

    #region Enumerable

    /// <summary>
    /// Performs the specified action on each element of the source sequence and yields the element.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to perform on each element.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that yields the elements of the source sequence after performing the action on each element.</returns>
    public static IEnumerable<T> Tap<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }

    #endregion


}
