using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SimLynx;

/// <summary>
/// An immutable set of symbols, optimized for fast comparisons.
/// </summary>
/// <param name="symbols">The symbols to include in the set.</param>
public class SymbolSet(IEnumerable<Symbol> symbols) :
#if NET8_0_OR_GREATER
    IReadOnlySet<Symbol>
#else
    IReadOnlyCollection<Symbol>
#endif
{
    private readonly ImmutableSortedSet<Symbol> _symbols = ImmutableSortedSet.CreateRange(
        Symbol.ValueComparer.Default,
        symbols
    );

    /// <inheritdoc/>
    public int Count => _symbols.Count;

    /// <inheritdoc/>
    public bool Contains(Symbol item) => _symbols.Contains(item);

    /// <inheritdoc/>
    public IEnumerator<Symbol> GetEnumerator() => _symbols.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _symbols.GetEnumerator();

    /// <inheritdoc/>
    public bool IsProperSubsetOf(IEnumerable<Symbol> other) => _symbols.IsProperSubsetOf(other);

    /// <inheritdoc/>
    public bool IsProperSupersetOf(IEnumerable<Symbol> other) => _symbols.IsProperSupersetOf(other);

    /// <inheritdoc/>
    public bool IsSubsetOf(IEnumerable<Symbol> other) => _symbols.IsSubsetOf(other);

    /// <inheritdoc/>
    public bool IsSupersetOf(IEnumerable<Symbol> other) => _symbols.IsSupersetOf(other);

    /// <inheritdoc/>
    public bool Overlaps(IEnumerable<Symbol> other) => _symbols.Overlaps(other);

    /// <inheritdoc/>
    public bool SetEquals(IEnumerable<Symbol> other) => _symbols.SetEquals(other);
}
