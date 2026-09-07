using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SimLynx;

/// <summary>
/// A key which uniquely identifies a type and optional additional "tag" symbol.
/// </summary>
/// <remarks>
/// This type can be used when type alone isn't sufficiently unique to identify objects, and is safe for (and designed
/// for) use as a dictionary key.
/// <br/>
/// For the purposes of this type, <see cref="Identifier.Empty"/> (and <see cref="Symbol.Empty"/>) is a valid and
/// distinct ID, representing the absence of one.
/// </remarks>
/// <param name="Type">The underlying system type.</param>
/// <param name="Id">The identifier of the type key.</param>
public readonly record struct TypeKey(Type Type, Identifier Id)
{
    /// <summary>
    /// Character used to separate the type name from its tag in the string representation of a <see cref="TypeKey"/>.
    /// </summary>
    public const char SEPARATOR = '#';

    /// <summary>
    /// Implicitly converts a <paramref name="type"/> to a <see cref="TypeKey"/> with an empty tag.
    /// </summary>
    /// <param name="type">The underlying system type.</param>
    public static implicit operator TypeKey(Type type) => new(type, Identifier.Empty);

    /// <summary>
    /// Attempts to parse a string representation of a <see cref="TypeKey"/> into a <see cref="TypeKey"/> instance.
    /// <br/>
    /// The string representation is expected to be in the format of <c>TypeName#Id</c>, where <c>TypeName</c> is the
    /// name of the type and <c>Id</c> is the optional identifier. The <c>Id</c> part is optional, and if omitted,
    /// the resulting <see cref="TypeKey"/> will have an empty identifier. A trailing separator character without an ID will
    /// also result in an empty identifier.
    /// <br/>
    /// </summary>
    /// <param name="serialized">The string representation of the <see cref="TypeKey"/> to parse.</param>
    /// <param name="typeKey">The resulting <see cref="TypeKey"/> instance if parsing is successful; otherwise, <c>default</c>.</param>
    /// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string serialized, [MaybeNullWhen(false)] out TypeKey typeKey)
    {
        if (string.IsNullOrEmpty(serialized))
        {
            typeKey = default;
            return false;
        }

        var separatorIdx = serialized.IndexOf(SEPARATOR);
        var typeName = separatorIdx > 0 ? serialized[..separatorIdx] : serialized;

        if (AppDomain.CurrentDomain.TryFindType(typeName, out var type))
        {
            typeKey = new(type, Identifier.Parse(separatorIdx > 0 ? serialized[(separatorIdx + 1)..] : null));
            return true;
        }

        typeKey = default;
        return false;
    }

    /// <summary>
    /// Creates a new <see cref="TypeKey"/> for the given <paramref name="type"/> with an empty ID.
    /// </summary>
    /// <param name="type">The underlying system type.</param>
    public TypeKey(Type type)
        : this(type, Identifier.Empty) { }

    /// <summary>
    /// Attempts to serialize this <see cref="TypeKey"/> to a string representation.
    /// </summary>
    /// <remarks>
    /// Will return <c>true</c> and set a valid <paramref name="result"/> if the <see cref="Id"/> can be stringified
    /// (via <see cref="Identifier.TryStringify(out string)"/>) or is empty.
    /// <br/>
    /// Note that this method does <em>not</em> return the same string representation as <see cref="ToString()"/> in
    /// <em>any</em> case.
    /// </remarks>
    /// <param name="result">The serialized result</param>
    /// <returns>If serialization was successful and the result can be used; otherwise, <c>false</c>.</returns>
    public bool TrySerialize([NotNullWhen(true)] out string? result)
    {
        var typeName = Type.ToString();

        if (Id.IsEmpty)
        {
            result = typeName;
            return true;
        }

        if (Id.TryStringify(out var idStr))
        {
            result = $"{typeName}{SEPARATOR}{idStr}";
            return true;
        }

        result = null;
        return false;
    }

    /// <summary>
    /// Converts this type key to a string representation.
    /// </summary>
    /// <remarks>
    /// This method will always return a string representation, even if the tag is a unique symbol, but the result
    /// may not be meaningful and will contain additional marker characters.
    /// <br/>
    /// This method should <strong>never</strong> be used for serialization/storage purposes; use
    /// <see cref="TrySerialize(out string?)"/> instead.
    /// </remarks>
    /// <returns>The string representation of this type key.</returns>
    public override string ToString()
    {
        return $"{Type}{SEPARATOR}{Id}";
    }

    /// <summary>
    /// Comparer implementation for <see cref="TypeKey"/> values.
    /// </summary>
    public class Comparer(
        IEqualityComparer<Identifier>? idComparer = null,
        IEqualityComparer<Type>? typeComparer = null
    ) : IEqualityComparer<TypeKey>
    {
        /// <summary>
        /// The singleton instance of a comparer that performs standard equality comparison on its tags.
        /// </summary>
        public static Comparer Default { get; } = new();

        /// <summary>
        /// The equality comparer used for comparing identifiers.
        /// </summary>
        public IEqualityComparer<Identifier> IdComparer { get; } = idComparer ?? Identifier.Comparer.Default;

        /// <summary>
        /// The equality comparer used for comparing types.
        /// </summary>
        public IEqualityComparer<Type> TypeComparer { get; } = typeComparer ?? EqualityComparer<Type>.Default;

        /// <inheritdoc/>
        public bool Equals(TypeKey x, TypeKey y)
        {
            if (!TypeComparer.Equals(x.Type, y.Type))
            {
                return false;
            }

            if (!IdComparer.Equals(x.Id, y.Id))
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public int GetHashCode(TypeKey obj)
        {
            return HashCode.Combine(TypeComparer.GetHashCode(obj.Type), IdComparer.GetHashCode(obj.Id));
        }
    }
}

/// <summary>
/// A generic type key
/// </summary>
/// <typeparam name="T">The type associated with this type key.</typeparam>
/// <param name="Id">The identifier for the type key.</param>
public readonly record struct TypeKey<T>(Identifier Id)
{
    /// <summary>
    /// Creates a new <see cref="TypeKey{T}"/> for the given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The ID associated with the type key.</param>
    public static implicit operator TypeKey<T>(Identifier id) => new(id);

    /// <inheritdoc cref="TypeKey{T}.op_Implicit(Identifier)"/>
    public static implicit operator TypeKey<T>(Symbol id) => new(id);

    /// <inheritdoc cref="TypeKey{T}.op_Implicit(Identifier)"/>
    public static implicit operator TypeKey<T>(string id) => new(id);

    /// <summary>
    /// Converts a <see cref="TypeKey{T}"/> to its non-generic <see cref="TypeKey"/> form.
    /// </summary>
    /// <param name="key"></param>
    public static explicit operator TypeKey(TypeKey<T> key) => new(typeof(T), key.Id);

    /// <summary>
    /// Creates a new <see cref="TypeKey{T}"/> with an empty ID.
    /// </summary>
    public TypeKey()
        : this(Identifier.Empty) { }
}
