using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace SimLynx;

/// <summary>
/// A symbolic identifier, used to identify objects with name-like keys, but with better performance than strings
/// for equality checks and dictionary lookups. Symbols are immutable and can be quickly compared for equality by-value.
/// <br/>
/// This type works similarly to JavaScript Symbols, coming in two varieties:
/// <list type="bullet">
///     <item><term>Named</term> <description>The symbol's description is used as an identifier; two symbols with the same description are equal.</description></item>
///     <item><term>Unique</term> <description>Each created symbol is unique and never equal to anything other than itself.</description></item>
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
/// By convention, symbol descriptions, should be <c>camelCase</c>. Named symbols are case-sensitive.
/// </remarks>
public readonly struct Symbol : IEquatable<Symbol>
{
    /// <summary>
    /// The maximum length of a symbol name.
    /// </summary>
    public const int MAX_NAME_LENGTH = 128;

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
    /// A unique symbol that can be used as a wildcard.
    /// </summary>
    public static readonly Symbol Any = new("any");

    /// <summary>
    /// A unique symbol that, unlike <see cref="Empty"/>, explicitly indicates "no value" and can be used as a
    /// "negative wildcard" or sentinel value.
    /// </summary>
    public static readonly Symbol Never = new("never");

    /// <summary>
    /// Equality operator for symbols.
    /// </summary>
    public static bool operator ==(Symbol left, Symbol right) => left.Equals(right);

    /// <summary>
    /// Inequality operator for symbols.
    /// </summary>
    public static bool operator !=(Symbol left, Symbol right) => !left.Equals(right);

    /// <summary>
    /// Implicit conversion from Symbol to string, retrieving the <see cref="Description"/> of the symbol.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    public static implicit operator string(Symbol symbol) => symbol.ToString();

    /// <summary>
    /// Implicit conversion from string to Symbol, creating a named symbol with the given name.
    /// </summary>
    public static implicit operator Symbol(string? name) => For(name);

    /// <summary>
    /// Combines two symbols into composite symbol, using the first symbol as the scope and the second as the value.
    /// </summary>
    /// <param name="qualifier">The scope symbol.</param>
    /// <param name="specifier">The value symbol.</param>
    /// <returns>The combined <see cref="Symbol"/> instance.</returns>
    public static Identifier operator +(Symbol qualifier, Symbol specifier) => new(qualifier, specifier);

    /// <summary>
    /// Creates or retrieves a named <see cref="Symbol"/> instance.
    /// </summary>
    /// <param name="name">The name of the symbol.</param>
    /// <returns>The named <see cref="Symbol"/> instance.</returns>
    public static Symbol For(string? name)
    {
        return new(Registry.GetNamed(name));
    }

    private readonly Registry.Entry? value;

    /// <summary>
    /// Creates a new unique <see cref="Symbol"/>.
    /// </summary>
    /// <remarks>
    /// Two new unique symbols, will never be considered equal.
    /// </remarks>
    public Symbol()
        : this(Registry.Get(null)) { }

    /// <summary>
    /// Creates a new unique <see cref="Symbol"/>, optionally with a description.
    /// </summary>
    /// <remarks>
    /// Two new unique symbols, even if created with the same description, will never be considered equal.
    /// </remarks>
    /// <param name="description">The description of the symbol</param>
    public Symbol(string? description)
        : this(Registry.Get(description)) { }

    private Symbol(Registry.Entry? entry)
    {
        value = entry;
    }

    /// <summary>
    /// Indicates whether this symbol is a named symbol, meaning it was created with a specific name and can be retrieved
    /// again using <see cref="For(string)"/>.
    /// </summary>
    /// <remarks>
    /// Named symbols always have a non-null <see cref="Description"/>.
    /// </remarks>
    [MemberNotNullWhen(true, nameof(Description))]
    public bool IsNamed => value?.IsNamed ?? false;

    /// <summary>
    /// The description of this symbol, if any.
    /// </summary>
    public string? Description => value?.Description;

    /// <summary>
    /// Indicates whether this symbol is equal to another symbol.
    /// </summary>
    /// <param name="other">The other symbol to check</param>
    /// <returns>If this symbol is equal to the other symbol</returns>
    public bool Equals(Symbol other)
    {
        return ReferenceEquals(value, other.value);
    }

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Symbol other && Equals(other);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"$({Description ?? string.Empty})";
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return value?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Equality comparer for symbols.
    /// </summary>
    public class Comparer : IEqualityComparer<Symbol>
    {
        /// <summary>
        /// The default symbol comparer.
        /// </summary>
        public static readonly Comparer Default = new();

        /// <inheritdoc/>
        public bool Equals(Symbol x, Symbol y)
        {
            return x.Equals(y);
        }

        /// <inheritdoc/>
        public int GetHashCode([DisallowNull] Symbol obj)
        {
            return obj.GetHashCode();
        }
    }

    private static class Registry
    {
        private static readonly ConcurrentBag<Entry> _entries = []; // keep a reference to all entries so they don't get GC'd
        private static readonly ConcurrentDictionary<string, Entry> _named = [];

        public static Entry Get(string? description)
        {
            return Add(new() { Description = description });
        }

        public static Entry GetNamed(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Entry.Empty;
            }

            return _named.GetOrAdd(name, _ => Add(new() { Description = name }));
        }

        private static Entry Add(Entry entry)
        {
            _entries.Add(entry);
            return entry;
        }

        internal class Entry
        {
            private static int _nextId = 0;

            public static readonly Entry Empty = new();

            public readonly int _id = Interlocked.Increment(ref _nextId);

            [MemberNotNullWhen(true, nameof(Description))]
            public bool IsNamed { get; init; }
            public string? Description { get; init; }

            public override int GetHashCode()
            {
                return _id;
            }

            public override bool Equals(object? obj)
            {
                return ReferenceEquals(this, obj);
            }
        }
    }
}
