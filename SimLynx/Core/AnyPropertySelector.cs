using System.Reflection;
using Autofac.Core;

namespace SimLynx.Core;

/// <summary>
/// Autofac property selector that selects all properties for injection.
/// </summary>
public class AnyPropertySelector : IPropertySelector
{
    /// <summary>
    /// The singleton instance of the <see cref="AnyPropertySelector"/> class.
    /// </summary>
    public static AnyPropertySelector Instance { get; } = new AnyPropertySelector();

    private AnyPropertySelector() { }

    /// <inheritdoc/>
    public bool InjectProperty(PropertyInfo propertyInfo, object instance)
    {
        return true;
    }
}
