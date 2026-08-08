namespace SimLynx.Core;

/// <summary>
/// A parameter which provides a value for a named property. The factory is invoked for each injection.
/// </summary>
/// <param name="name">The name of the property to inject.</param>
/// <param name="valueFactory">The factory function used to create property values.</param>
public class NamedPropertyProviderParameter(string name, PropertyProviderParameter.ValueFactory valueFactory)
    : PropertyProviderParameter(prop => prop.Name == name, valueFactory)
{
    /// <summary>
    /// The name of the property to inject.
    /// </summary>
    public string Name { get; } = name;
}
