using System.Linq;
using System.Reflection;
using Autofac.Core;

namespace SimLynx.Core.Prototyping;

internal class PrototypePropertySelector : IPropertySelector
{
    private static readonly PropertyInfo[] _injectableProps =
    [
        typeof(IPrototype).GetProperty(nameof(IPrototype.Base))!,
        typeof(IPrototype).GetProperty(nameof(IPrototype.IsAbstract))!,
    ];

    public static PrototypePropertySelector Instance { get; } = new();

    public bool InjectProperty(PropertyInfo propertyInfo, object instance)
    {
        return _injectableProps.Contains(propertyInfo);
    }
}
