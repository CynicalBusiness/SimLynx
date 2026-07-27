using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;

namespace SimLynx.Core;

/// <summary>
/// Autofac property selector that selects properties by name for injection.
/// </summary>
/// <param name="propertyNames">The names of the properties to select for injection.</param>
public class NamedPropertySelector(IEnumerable<string> propertyNames) : IPropertySelector
{
    private readonly HashSet<string> _propertyNames = [.. propertyNames];

    /// <inheritdoc/>
    public bool InjectProperty(PropertyInfo propertyInfo, object instance)
    {
        return _propertyNames.Contains(propertyInfo.Name);
    }
}
