using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SimLynx;

/// <summary>
/// A qualifiable identifier that may be used as a high-performance key for dictionaries and other data structures.
/// </summary>
/// <param name="Qualifier">The qualifier part of the identifier.</param>
/// <param name="Specifier">The specifier part of the identifier.</param>
public readonly record struct Identifier(Symbol Qualifier, Symbol Specifier)
{
    /// <summary>
    /// The delimiter character used to separate the qualifier and specifier parts of an identifier when converting
    /// to/from a string.
    /// </summary>
    public const char DELIMITER = ':';

    /// <summary>
    /// Implicitly converts a <see cref="Symbol"/> to an <see cref="Identifier"/>, using the symbol as the specifier
    /// and an empty qualifier.
    /// </summary>
    /// <param name="specifier">The specifier part of the identifier.</param>
    public static implicit operator Identifier(Symbol specifier) => new(specifier);

    /// <summary>
    /// Implicitly converts a <c>string</c> to an <see cref="Identifier"/> with <see cref="Parse(string?)"/>.
    /// </summary>
    /// <param name="str">The string representation to parse</param>
    public static implicit operator Identifier(string? str) => Parse(str);

    /// <summary>
    /// An empty identifier with no qualifier and no specifier.
    /// </summary>
    public static Identifier Empty { get; } = default;

    /// <summary>
    /// Attempts to parse a string into an <see cref="Identifier"/>.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Identifier Parse(string? str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return Empty;
        }

        // perhaps we want to include an escape for the delimiter in the future?
        var delimiterIdx = str.IndexOf(DELIMITER);
        if (delimiterIdx < 0)
        {
            return new(str);
        }
        else if (delimiterIdx == 0)
        {
            return new(str[1..]);
        }
        else
        {
            return new(str[..delimiterIdx], str[(delimiterIdx + 1)..]);
        }
    }

    /// <summary>
    /// Constructs a new identifier with no (empty) qualifier, using the given <paramref name="specifier"/>.
    /// </summary>
    /// <param name="specifier">The specifier part of the identifier.</param>
    public Identifier(Symbol specifier)
        : this(Symbol.Empty, specifier) { }

    /// <summary>
    /// Indicates whether this identifier has a non-empty qualifier.
    /// </summary>
    public bool IsQualified => Qualifier != Symbol.Empty;

    /// <summary>
    /// Indicates whether this identifier has a non-empty specifier.
    /// </summary>
    public bool IsSpecified => Specifier != Symbol.Empty;

    /// <summary>
    /// Indicates whether this identifier is empty (has no qualifier and no specifier).
    /// </summary>
    public bool IsEmpty => !IsQualified && !IsSpecified;

    /// <summary>
    /// Attempts to convert this identifier into a string representation, using the delimiter character to separate the
    /// qualifier and specifier parts if the qualifier is non-empty. The conversion will only be successful if the
    /// specifier is named and either the qualifier is also named or empty.
    /// </summary>
    /// <remarks>
    /// While identifiers with unique symbols are valid for internal/runtime use, they do not have stable string
    /// representations, as the symbols themselves do not.
    /// </remarks>
    /// <param name="str">The resulting string representation if the conversion is successful; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the conversion is successful; otherwise, <c>false</c>.</returns>
    public bool TryStringify([MaybeNullWhen(false)] out string str)
    {
        if (Specifier.IsNamed)
        {
            if (Qualifier.IsNamed)
            {
                str = $"{Qualifier.Description}{DELIMITER}{Specifier.Description}";
                return true;
            }
            else if (!IsQualified)
            {
                str = Specifier.Description;
                return true;
            }
        }

        str = null;
        return false;
    }

    /// <summary>
    /// Returns a string representation of this identifier, using the delimiter character to separate the qualifier
    /// and specifier parts if the qualifier is non-empty.
    /// </summary>
    /// <remarks>
    /// Like <see cref="Symbol"/>, the result string is for display/debugging purposes and should not be relied upon
    /// for serialization or other stable storage.
    /// </remarks>
    /// <returns>A string representation of this identifier.</returns>
    public override string ToString()
    {
        return "@(" + (IsEmpty ? string.Empty : $"{Qualifier}{DELIMITER}{Specifier}") + ")";
    }

    /// <summary>
    /// Comparer for <see cref="Identifier"/> values.
    /// </summary>
    /// <param name="symbolComparer">The comparer used to compare the qualifier and specifier symbols of identifiers.</param>
    public class Comparer(IEqualityComparer<Symbol>? symbolComparer) : IEqualityComparer<Identifier>
    {
        /// <summary>
        /// The default comparer for identifiers, which uses <see cref="Symbol.Comparer.Default"/> for comparing
        /// the qualifier and specifier symbols.
        /// </summary>
        public static Comparer Default { get; } = new(null);

        /// <summary>
        /// The comparer used to compare the qualifier and specifier symbols of identifiers.
        /// </summary>
        public IEqualityComparer<Symbol> SymbolComparer { get; } = symbolComparer ?? Symbol.Comparer.Default;

        /// <inheritdoc/>
        public bool Equals(Identifier x, Identifier y)
        {
            return SymbolComparer.Equals(x.Qualifier, y.Qualifier) && SymbolComparer.Equals(x.Specifier, y.Specifier);
        }

        /// <inheritdoc/>
        public int GetHashCode(Identifier obj)
        {
            return HashCode.Combine(
                SymbolComparer.GetHashCode(obj.Qualifier),
                SymbolComparer.GetHashCode(obj.Specifier)
            );
        }
    }
}
