using System;

namespace SimLynx.Core;

/// <summary>
/// Interface for a meta-type, which is a helper that represents a system <see cref="Type"/>, but allows for type-safe
/// handling of that type, as well as cached metadata about it.
/// </summary>
public interface IMetaType
{
    /// <summary>
    /// The underlying system type of this meta-type.
    /// </summary>
    public Type Type { get; }
}

/// <summary>
/// A <see cref="IMetaType"/> of a <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type represented by this meta-type.</typeparam>
public interface IMetaType<out T> : IMetaType { }
