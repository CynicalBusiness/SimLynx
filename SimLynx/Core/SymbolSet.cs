
using System.Collections;
using System.Collections.Generic;

namespace SimLynx.Core;

/// <summary>
/// A set of symbols, generally used like a "set of tags"
/// </summary>
public class SymbolSet : IReadOnlySet<Symbol>
{
    // ? lazy implementation for now

    private readonly HashSet<Symbol> _symbols = [];

    /// <inheritdoc/>
    public int Count => _symbols.Count;

    /// <inheritdoc/>
    public bool Contains(Symbol item)
    {
        return _symbols.Contains(item);
    }

    /// <inheritdoc/>
    public IEnumerator<Symbol> GetEnumerator()
    {
        return _symbols.GetEnumerator();
    }

    /// <inheritdoc/>
    public bool IsProperSubsetOf(IEnumerable<Symbol> other)
    {
        return _symbols.IsProperSubsetOf(other);
    }

    /// <inheritdoc/>
    public bool IsProperSupersetOf(IEnumerable<Symbol> other)
    {
        return _symbols.IsProperSupersetOf(other);
    }

    /// <inheritdoc/>
    public bool IsSubsetOf(IEnumerable<Symbol> other)
    {
        return _symbols.IsSubsetOf(other);
    }

    /// <inheritdoc/>
    public bool IsSupersetOf(IEnumerable<Symbol> other)
    {
        return _symbols.IsSupersetOf(other);
    }

    /// <inheritdoc/>
    public bool Overlaps(IEnumerable<Symbol> other)
    {
        return _symbols.Overlaps(other);
    }

    /// <inheritdoc/>
    public bool SetEquals(IEnumerable<Symbol> other)
    {
        return _symbols.SetEquals(other);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
