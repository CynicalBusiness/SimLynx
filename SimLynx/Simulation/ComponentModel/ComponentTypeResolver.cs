using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;

namespace SimLynx.Simulation.ComponentModel;

internal sealed class ComponentTypeResolver(IComponentContext container) : IComponentTypeResolver
{
    public IEnumerable<IComponentTypeConfig> GetConfigs(Type componentType)
    {
        if (!componentType.IsAssignableTo(typeof(Component)))
        {
            throw new ArgumentException($"Type '{componentType}' is not a component type.", nameof(componentType));
        }

        return GetConfigsInternal(componentType);
    }

    public IEnumerable<IComponentTypeConfig<TComponent>> GetConfigs<TComponent>()
        where TComponent : Component
    {
        // Can't cast directly, but IComponentTypeConfig is contravariant, so we can cast each one to the most specific
        return GetConfigs(typeof(TComponent)).Cast<IComponentTypeConfig<TComponent>>();
    }

    private IEnumerable<IComponentTypeConfig> GetConfigsInternal(Type componentType)
    {
        // skips redundant validation

        var resolveType = typeof(IEnumerable<>).MakeGenericType(
            typeof(IComponentTypeConfig<>).MakeGenericType(componentType)
        );
        var currentConfigs = (IEnumerable<IComponentTypeConfig>)container.Resolve(resolveType); // IEnumerable is covariant, can safely cast

        if (componentType != typeof(Component))
        {
            // BaseType is safe as we already validated that this is a Component type
            currentConfigs = GetConfigsInternal(componentType.BaseType!).Concat(currentConfigs);
        }

        return currentConfigs;
    }
}
