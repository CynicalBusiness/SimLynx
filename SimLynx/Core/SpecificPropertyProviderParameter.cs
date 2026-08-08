using System.Reflection;

namespace SimLynx.Core;

/// <summary>
/// A parameter which provides a value for a specific property. The factory is invoked for each injection.
/// </summary>
/// <param name="property">The property for which this parameter provides a value.</param>
/// <param name="valueFactory">The factory to provide the value for the property.</param>
public class SpecificPropertyProviderParameter(
    PropertyInfo property,
    PropertyProviderParameter.ValueFactory valueFactory
) : PropertyProviderParameter((p) => p == property, valueFactory)
{
    /// <summary>
    /// The property for which this parameter provides a value.
    /// </summary>
    public PropertyInfo Property { get; } = property;
}
