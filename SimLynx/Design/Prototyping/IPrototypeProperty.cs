using System;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Generalized property information for a single property on a prototype's target type.
/// </summary>
public interface IPrototypeProperty
{
    /// <summary>
    /// The type of the property on the prototype's target type.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// The name of the property on the prototype's target type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Indicates whether this property is required to be defined for this prototype to compile.
    /// </summary>
    public bool IsRequired { get; }

    /// <summary>
    /// Indicates whether this property has been configured with any values or actions.
    /// </summary>
    public bool IsConfigured { get; }
}
