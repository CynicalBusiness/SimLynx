using System.Linq;
using System.Reflection;
using Autofac.Core;

namespace SimLynx.Design.Prototyping;

internal class PrototypePropertySelector : IPropertySelector
{
    private static readonly string[] _injectableProps = [nameof(IPrototype.Base), nameof(IPrototype.IsAbstract)];

    public static PrototypePropertySelector Instance { get; } = new();

    public bool InjectProperty(PropertyInfo propertyInfo, object instance)
    {
        return _injectableProps.Contains(propertyInfo.Name);
    }
}
