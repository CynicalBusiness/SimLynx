
using System;

namespace SimLynx.Core;

/// <summary>
/// Interface for reflection type info.
/// </summary>
/// <remarks>
/// Type info is useful to cache reflection information about types to avoid repeated reflection calls, as well
/// as provide an abstraction for working with that information.
/// </remarks>
public interface ITypeInfo
{

    /// <summary>
    /// The underlying system type this type info represents.
    /// </summary>
    public Type Type { get; }

}

/// <summary>
/// Generic version of <see cref="ITypeInfo"/> which constrains the type it represents for type-safety.
/// </summary>
/// <remarks>
/// Note that <typeparamref name="TType"/> may not necessarily be the same as <see cref="ITypeInfo.Type"/>, but
/// must be assignable <em>from</em> it. That is, <typeparamref name="TType"/> can be a base class or interface in
/// which the <see cref="ITypeInfo.Type"/> implements or inherits from. This allows for instances manufactured
/// by this type info to be typed as <typeparamref name="TType"/>, even if the actual system type is more specific.
/// </remarks>
/// <typeparam name="TType">The type this type info represents, or a type is is otherwise assignable to.</typeparam>
public interface ITypeInfo<out TType> : ITypeInfo
    where TType : notnull
{

}
