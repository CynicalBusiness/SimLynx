using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SimLynx;

/// <summary>
/// A symbolic identifier, used to identify objects with name-like objects, but with better performance than strings
/// for equality checks and dictionary lookups. Symbols are immutable and can be quickly compared for equality by-value.
/// <br/>
/// This type works similarly to JavaScript Symbols, coming in two varieties:
/// <list type="bullet">
///     <item><term>Named</term> <description>Two symbols created with the same name are considered equal.</description></item>
///     <item><term>Unique</term> <description>Each symbol is unique, even if created with the same name.</description></item>
/// </list>
/// </summary>
/// <remarks>
/// A Symbol's main advantage over a string is its comparison speed: comparing two symbols for equality is a single
/// integer comparison, and hashing is essentially free (since the Symbol's value is returned as-is). Compare that
/// to strings, which require a full string comparison and hash computation. This makes symbols ideal for use as
/// dictionary keys or for other scenarios where equality checks are frequent.
/// <br/>
/// It should be avoided, however, to build symbols "on-the-fly", as named Symbol *creation* is not free:
/// <see cref="For"/> runs a fast but non-trivial hash computation and can block threads during lookup dictionary
/// access. Consider defining relevant symbols in advance, such as `static` members, and reusing them instead of
/// creating new symbols at runtime.
/// <br/>
/// By convention, symbol descriptions/names, if any, should be <c>camelCase</c>, and are case-sensitive.
/// </remarks>
public readonly struct Symbol : IEquatable<Symbol>
{
    /// <summary>
    /// The maximum length of a symbol name.
    /// </summary>
    public const int MAX_NAME_LENGTH = 128;

    /// <summary>
    /// The prefix character used to denote symbol names in string representations.
    /// </summary>
    public const string PREFIX = "@";

    /// <summary>
    /// The prefix used to denote unique symbols in string representations.
    /// </summary>
    public const string UNIQUE_PREFIX = "~";

    /// <summary>
    /// The empty symbol instance (an unnamed unique symbol).
    /// </summary>
    /// <remarks>
    /// This property is equivalent to <c>default(Symbol)</c>, and is provided for convenience and readability.
    /// </remarks>
    public static readonly Symbol Empty = default;

    /// <summary>
    /// A unique symbol that can be used to identify objects that are internal to their respective owners and
    /// are not intended to be seen/used by external consumers.
    /// </summary>
    public static readonly Symbol Internal = new("internal");

    /// <summary>
    /// Equality operator for symbols.
    /// </summary>
    public static bool operator ==(Symbol left, Symbol right) => left.Equals(right);

    /// <summary>
    /// Inequality operator for symbols.
    /// </summary>
    public static bool operator !=(Symbol left, Symbol right) => !left.Equals(right);

    /// <summary>
    /// Explicit conversion from string to Symbol, creating or retrieving a named symbol with the given name.
    /// <br/>
    /// This is equivalent to calling <see cref="For(string)"/> with the provided name.
    /// </summary>
    /// <param name="name">The name of the symbol.</param>
    public static explicit operator Symbol(string name) => For(name);

    /// <summary>
    /// Implicit conversion from Symbol to string, retrieving the <see cref="Description"/> of the symbol.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    public static implicit operator string(Symbol symbol) => symbol.ToString();

    /// <summary>
    /// Implicit conversion from Symbol to int, retrieving the <see cref="Value"/> of the symbol.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    public static implicit operator int(Symbol symbol) => symbol.Value;

    /// <summary>
    /// Implicit conversion from Symbol to EventId, creating a new EventId with the symbol's value and name.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    [Obsolete(
        "Implicit conversion from Symbol to EventId is deprecated. Symbol.Value is not stable and should not be used as an EventId."
    )]
    public static implicit operator EventId(Symbol symbol) => new(symbol.Value, symbol.Description);

    private static readonly ConcurrentDictionary<int, string> nameRegistry = new() { [Empty.Value] = string.Empty };

    private static int _uniqueValueIdx = -1;

    /// <summary>
    /// Creates or retrieves a named <see cref="Symbol"/> instance.
    /// </summary>
    /// <param name="name">The name of the symbol.</param>
    /// <returns>The named <see cref="Symbol"/> instance.</returns>
    public static Symbol For(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return Empty;
        }

        int hash = ComputeHashCode(name);
        nameRegistry.TryAdd(hash, name);
        return new Symbol(hash);
    }

    private static int ComputeHashCode(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return 0;
        }
        if (str.Length > MAX_NAME_LENGTH)
        {
            throw new ArgumentException(
                $"Symbol name '{str}' exceeds maximum length of {MAX_NAME_LENGTH} characters.",
                nameof(str)
            );
        }
        unchecked
        {
            // https://mojoauth.com/hashing/fast-hash-in-c/
            // speed and stability are more important than cryptographic quality here
            int hash = 17;
            foreach (char c in str)
            {
                hash = hash * 31 + c;
            }
            return hash & int.MaxValue; // hash codes should be non-negative to stay in "named" space.
        }
    }

    /// <summary>
    /// The numerical value of the symbol.
    /// </summary>
    /// <remarks>
    /// Of note, the "value" of a symbol is an implementation detail and not guaranteed to be stable across different
    /// runs of the application, and so should not be used for serialization. Named symbols should be
    /// stored/transmitted by name, and unique symbols are, by design, not serializable.
    /// </remarks>
    public int Value { get; }

    /// <summary>
    /// Creates a new unique <see cref="Symbol"/>.
    /// </summary>
    /// <remarks>
    /// Two new unique symbols, will never be considered equal.
    /// </remarks>
    public Symbol()
        : this(Interlocked.Decrement(ref _uniqueValueIdx)) // should cause an overflow exception if we ever run out of unique symbols
    { }

    /// <summary>
    /// Creates a new unique <see cref="Symbol"/>, optionally with a description.
    /// </summary>
    /// <remarks>
    /// Two new unique symbols, even if created with the same description, will never be considered equal.
    /// </remarks>
    /// <param name="description">The description of the symbol</param>
    public Symbol(string? description)
        : this()
    {
        Description = description;
    }

    private Symbol(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Indicates whether this symbol is unique (not named).
    /// </summary>
    [MemberNotNullWhen(false, nameof(Description))]
    public bool IsUnique => Value <= 0;

    /// <summary>
    /// The description of this symbol, if any.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Indicates whether this symbol is equal to another symbol.
    /// </summary>
    /// <param name="other">The other symbol to check</param>
    /// <returns>If this symbol is equal to the other symbol</returns>
    public bool Equals(Symbol other)
    {
        return Value == other.Value;
    }

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Symbol other && Equals(other);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return PREFIX + (IsUnique ? UNIQUE_PREFIX + (Description ?? $"<{Value:X}>") : Description);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value;
    }

    /// <summary>
    /// Compares two symbols by their <see cref="Value"/> property, and provides equality comparison and hashing for symbols.
    /// </summary>
    public class ValueComparer : IComparer<Symbol>, IEqualityComparer<Symbol>
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="ValueComparer"/> class.
        /// </summary>
        public static ValueComparer Default { get; } = new();

        private ValueComparer() { }

        /// <inheritdoc/>
        public int Compare(Symbol x, Symbol y)
        {
            return x.Value.CompareTo(y.Value);
        }

        /// <inheritdoc/>
        public bool Equals(Symbol x, Symbol y)
        {
            return x.Equals(y);
        }

        /// <inheritdoc/>
        public int GetHashCode(Symbol obj)
        {
            return obj.GetHashCode();
        }
    }
}
