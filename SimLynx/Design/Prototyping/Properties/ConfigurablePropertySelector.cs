using System.Reflection;
using Autofac.Core;

namespace SimLynx.Design.Prototyping.Properties;

/// <summary>
/// A property selector that allows properties which are <see cref="PrototypePropertyExtensions.get_IsPrototypeConfigurable"/>.
/// </summary>
public class ConfigurablePropertySelector : IPropertySelector
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static ConfigurablePropertySelector Instance { get; } = new();

    private ConfigurablePropertySelector() { }

    /// <inheritdoc/>
    public bool InjectProperty(PropertyInfo propertyInfo, object instance)
    {
        return propertyInfo.IsPrototypeConfigurable;
    }
}
