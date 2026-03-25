using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// Reflection information about a prototype type.
/// </summary>
public interface IPrototypeInfo : ITypeInfo
{
    /// <summary>
    /// Gets the property with the given name, if it exists in this prototype type's properties, including inherited
    /// properties from base prototype types.
    /// </summary>
    /// <param name="propertyName">The property name to get</param>
    /// <returns>The property with the specified name, if it exists; otherwise, <c>null</c>.</returns>
    public PrototypeProperty? this[string propertyName] { get; }

    /// <summary>
    /// The <em>own</em> properties of this prototype type, keyed by property name. Does not include, for example,
    /// any properties inherited from base types.
    /// </summary>
    public IReadOnlyDictionary<string, PrototypeProperty> OwnProperties { get; }

    /// <summary>
    /// All properties of this prototype type, including inherited properties from base prototype types.
    /// </summary>
    public IEnumerable<PrototypeProperty> Properties { get; }

    /// <summary>
    /// Whether the prototype type this represents is abstract (or otherwise cannot be instantiated).
    /// </summary>
    public bool IsAbstract { get; }

    /// <summary>
    /// Gets the property with the given name, if it exists in this prototype type's properties, including inherited properties
    /// from base prototype types.
    /// </summary>
    /// <param name="name">The name of the property to get.</param>
    /// <param name="property">When this method returns, contains the property with the specified name, if it exists; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the property exists; otherwise, <c>false</c>.</returns>
    public bool TryGetProperty(string name, [NotNullWhen(true)] out PrototypeProperty? property);
}

/// <summary>
/// Reflection information about a <typeparamref name="TType"/> prototype type.
/// </summary>
/// <typeparam name="TType">The type of the prototype.</typeparam>
public interface IPrototypeInfo<out TType> : IPrototypeInfo, ITypeInfo<TType>
    where TType : class, IPrototype
{
    /// <summary>
    /// Function responsible for creating an instance of the prototype type this represents.
    /// </summary>
    /// <remarks>
    /// This function is <c>null</c> if the prototype type cannot be constructed, such as if it is abstract or lacks a
    /// suitable constructor.
    /// </remarks>
    public Func<TType>? CreateInstance { get; }

    /// <inheritdoc cref="IPrototypeInfo.IsAbstract"/>
    [MemberNotNullWhen(false, nameof(CreateInstance))]
    public new bool IsAbstract => CreateInstance is null;
    bool IPrototypeInfo.IsAbstract => IsAbstract;
}
