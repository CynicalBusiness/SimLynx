using System;

namespace SimLynx;

/// <summary>
/// A "meta-type" is a helper which contains metadata about a particular type, such as cached reflection information,
/// where retrieving that information on-the-fly would be expensive.
/// <br/>
/// To get/create a meta-type, use <see cref="MetaType.For{T}"/> or <see cref="MetaType.For(Type)"/>.
/// </summary>
/// <remarks>
/// This interface works by providing extensions to access the data within <see cref="Metadata"/> and that is the
/// suggested way to further its features.
/// </remarks>
public interface IMetaType
{
    /// <summary>
    /// The underlying system type of this meta-type.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// The cache of metadata about this meta-type.
    /// </summary>
    public TypeDictionary Metadata { get; }
}

/// <summary>
/// A <see cref="IMetaType"/> of a <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type represented by this meta-type.</typeparam>
public interface IMetaType<out T> : IMetaType { }
