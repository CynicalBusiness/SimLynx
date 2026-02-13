
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SimLynx;

/// <summary>
/// A symbolic identifier, used to identify objects with name-like objects, but with better performance than strings.
/// <br/>
/// This type works similarly to JavaScript Symbols, coming in two varieties:
/// <list type="bullet">
///     <item><term>Named</term><description>Two symbols created with the same name are considered equal.</description></item>
///     <item><term>Unique</term><description>Each symbol is unique, even if created with the same name.</description></item>
/// </list>
/// </summary>
public readonly struct Symbol : IEquatable<Symbol>
{
    /// <summary>
    /// The maximum length of a symbol name.
    /// </summary>
    public const int MAX_NAME_LENGTH = 128;

    /// <summary>
    /// The name used for unknown symbol names.
    /// </summary>
    public const string UNKNOWN_NAME = "<unknown>";

    /// <summary>
    /// The prefix character used to denote symbol names in string representations.
    /// </summary>
    public const string SYMBOL_PREFIX = "#";

    /// <summary>
    /// The default symbol instance (an unnamed unique symbol).
    /// </summary>
    public static readonly Symbol Default = default;

    /// <summary>
    /// Equality operator for symbols.
    /// </summary>
    public static bool operator ==(Symbol left, Symbol right) => left.Equals(right);

    /// <summary>
    /// Inequality operator for symbols.
    /// </summary>
    public static bool operator !=(Symbol left, Symbol right) => !left.Equals(right);

    /// <summary>
    /// Implicit conversion from string to Symbol, creating or retrieving a named symbol with the given name.
    /// <br/>
    /// This is equivalent to calling <see cref="For(string)"/> with the provided name.
    /// </summary>
    /// <param name="name">The name of the symbol.</param>
    public static implicit operator Symbol(string name) => For(name);

    /// <summary>
    /// Implicit conversion from Symbol to string, retrieving the <see cref="Name"/> of the symbol.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    public static implicit operator string(Symbol symbol) => symbol.Name;

    /// <summary>
    /// Implicit conversion from Symbol to int, retrieving the <see cref="Value"/> of the symbol.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    public static implicit operator int(Symbol symbol) => symbol.Value;

    /// <summary>
    /// Implicit conversion from Symbol to EventId, creating a new EventId with the symbol's value and name.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    public static implicit operator EventId(Symbol symbol) => new(symbol.Value, symbol.Name);

    private static readonly ConcurrentDictionary<int, string> nameLookup = [];

    private static int _uniqueValueIdx = -1;

    /// <summary>
    /// Creates or retrieves a named <see cref="Symbol"/> instance.
    /// </summary>
    /// <param name="name">The name of the symbol.</param>
    /// <returns>The named <see cref="Symbol"/> instance.</returns>
    public static Symbol For(string name)
    {
        int hash = ComputeHashCode(name);
        nameLookup.TryAdd(hash, name);
        return new Symbol(hash);
    }

    private static int ComputeHashCode(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return 0;
        }
        if (name.Length > MAX_NAME_LENGTH)
        {
            throw new ArgumentException($"Symbol name exceeds maximum length of {MAX_NAME_LENGTH} characters.", nameof(name));
        }

        unchecked
        {
            // https://mojoauth.com/hashing/fast-hash-in-c/
            // speed and stability are more important than cryptographic quality here
            int hash = 17;
            foreach (char c in name)
            {
                hash = hash * 31 + c;
            }
            return hash & int.MaxValue; // hash codes should be non-negative to stay in "named" space.
        }
    }

    /// <summary>
    /// The numerical value of the symbol.
    /// </summary>
    public int Value { get; private init; }

    /// <summary>
    /// Creates a new unique <see cref="Symbol"/>.
    /// </summary>
    public Symbol()
        : this(Interlocked.Decrement(ref _uniqueValueIdx)) // should cause an overflow exception if we ever run out of unique symbols
    { }

    /// <summary>
    /// Creates a new unique <see cref="Symbol"/>, optionally with a name.
    /// </summary>
    /// <remarks>
    /// This creates a <em>unique</em> symbol with its own name, and will still create a new unique symbol even if the
    /// same name is provided. To create a <em>named</em> symbol, use <see cref="For(string)"/>.
    /// </remarks>
    /// <param name="name">The name of the symbol</param>
    public Symbol(string? name)
        : this()
    {
        if (!string.IsNullOrEmpty(name))
        {
            nameLookup.TryAdd(Value, name);
        }
    }

    private Symbol(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Indicates whether this symbol is unique (not named).
    /// </summary>
    public bool IsUnique => Value < 0;

    /// <summary>
    /// Indicates whether this symbol is named.
    /// </summary>
    public bool IsNamed => Value >= 0;

    /// <summary>
    /// Retrieves the name of this symbol.
    /// </summary>
    public string Name => nameLookup.GetValueOrDefault(Value, UNKNOWN_NAME);

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
        return $"{SYMBOL_PREFIX}{(IsUnique ? "~" : "")}{Name}";
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value;
    }

}
