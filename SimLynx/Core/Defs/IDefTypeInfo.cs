
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SimLynx.Core.Defs;

/// <summary>
/// Reflection information about a def type, used for configuring defs via "configuration" reflection.
/// </summary>
/// <typeparam name="TDef"></typeparam>
public interface IDefTypeInfo<out TDef> : ITypeInfo<TDef>
    where TDef : class, IDef
{

    /// <summary>
    /// Gets the property with the given name, if it exists in this def type's properties, including inherited
    /// properties from base def types.
    /// </summary>
    /// <param name="propertyName">The property name to get</param>
    /// <returns>The property with the specified name, if it exists; otherwise, <c>null</c>.</returns>
    public IDefProperty? this[string propertyName] { get; }

    /// <summary>
    /// The type of def this represents.
    /// </summary>
    public Type DefType { get; }
    Type ITypeInfo.Type => DefType;

    /// <summary>
    /// The own properties of this def type, keyed by property name. Does not include properties inherited from base
    /// def types.
    /// </summary>
    public IReadOnlyDictionary<string, IDefProperty> OwnProperties { get; }

    /// <summary>
    /// All properties of this def type, including inherited properties from base def types.
    /// </summary>
    public IEnumerable<IDefProperty> Properties { get; }

    /// <summary>
    /// Function responsible for creating an instance of the def type this represents.
    /// </summary>
    /// <remarks>
    /// This function is <c>null</c> if the def type cannot be constructed, such as if it is abstract or lacks a
    /// suitable constructor.
    /// </remarks>
    public Func<TDef>? CreateInstance { get; }

    /// <summary>
    /// Whether the def type this represents is abstract (or otherwise cannot be instantiated).
    /// </summary>
    [MemberNotNullWhen(false, nameof(CreateInstance))]
    public bool IsAbstract => CreateInstance is null;

    /// <summary>
    /// Gets the property with the given name, if it exists in this def type's properties, including inherited properties
    /// from base def types.
    /// </summary>
    /// <param name="name">The name of the property to get.</param>
    /// <param name="property">When this method returns, contains the property with the specified name, if it exists; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the property exists; otherwise, <c>false</c>.</returns>
    public bool TryGetProperty(string name, [NotNullWhen(true)] out IDefProperty? property);

}
