using System;

namespace SimLynx;

/// <summary>
/// A key which uniquely identifies a type and optional additional "tag" symbol.
/// </summary>
/// <remarks>
/// This type can be used when type alone isn't sufficiently unique to identify objects, and is safe for (and designed
/// for) use as a dictionary key.
/// <br/>
/// For the purposes of this type, <see cref="Symbol.Empty"/> is a valid and distinct tag, representing the absence of
/// one.
/// </remarks>
/// <param name="Type">The underlying system type.</param>
/// <param name="Tag">An optional additional "tag" symbol.</param>
public record TypeKey(Type Type, Symbol Tag)
{
    /// <summary>
    /// Implicitly converts a <paramref name="type"/> to a <see cref="TypeKey"/> with an empty tag.
    /// </summary>
    /// <param name="type">The underlying system type.</param>
    public static implicit operator TypeKey(Type type) => new(type, Symbol.Empty);

    /// <summary>
    /// Creates a new <see cref="TypeKey"/> for the given <paramref name="type"/> with an empty tag.
    /// </summary>
    /// <param name="type">The underlying system type.</param>
    public TypeKey(Type type)
        : this(type, Symbol.Empty) { }

    /// <summary>
    /// Indicates whether this <see cref="TypeKey"/> has a non-empty tag.
    /// </summary>
    public bool HasTag => Tag != Symbol.Empty;
}
