using System.Reflection;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// Contains information about a particular prototype's property.
/// </summary>
/// <param name="Prototype">The prototype this property belongs to.</param>
/// <param name="Property">The reflection information about the property.</param>
public record PrototypeProperty(IPrototypeInfo Prototype, PropertyInfo Property)
{
    /// <summary>
    /// Whether this property is required to be configured for the prototype to be valid.
    /// </summary>
    public bool IsRequired => Property.IsRequired;
}
