using System;
using System.Reflection;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Helper to get the <see cref="IComponentType"/> for a component type.
/// </summary>
public static class ComponentTypes
{
    private static readonly MethodInfo _genericForMethod = typeof(ComponentTypes).GetMethod(
        nameof(For),
        BindingFlags.Public | BindingFlags.Static,
        null,
        [],
        null
    )!;

    /// <summary>
    /// Gets the <see cref="IComponentType{TComponent}"/> for the <typeparamref name="TComponent"/> component.
    /// </summary>
    /// <typeparam name="TComponent">The type of the component.</typeparam>
    /// <returns>The <see cref="IComponentType{TComponent}"/> for the specified component type.</returns>
    public static IComponentType<TComponent> For<TComponent>()
        where TComponent : Component
    {
        return ComponentType<TComponent>.Instance;
    }

    /// <summary>
    /// Gets the <see cref="IComponentType"/> for the given component <paramref name="componentType"/>.
    /// The given type must derive from <see cref="Component"/>.
    /// </summary>
    /// <param name="componentType">The type of the component.</param>
    /// <returns>The <see cref="IComponentType"/> for the specified component type.</returns>
    /// <exception cref="ArgumentException">Thrown if the given type is null or does not derive from <see cref="Component"/>.</exception>
    public static IComponentType For(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType, nameof(componentType));

        if (!componentType.IsAssignableTo(typeof(Component)))
        {
            throw new ArgumentException(
                $"Type '{componentType.FullName}' is not a component type (does not derive from '{typeof(Component).FullName}')",
                nameof(componentType)
            );
        }

        return (IComponentType)_genericForMethod.MakeGenericMethod(componentType).Invoke(null, [])!;
    }
}

internal class ComponentType<TComponent> : IComponentType<TComponent>
    where TComponent : Component
{
    public static ComponentType<TComponent> Instance { get; } = new();

    public Type Type { get; } = typeof(TComponent);
}
