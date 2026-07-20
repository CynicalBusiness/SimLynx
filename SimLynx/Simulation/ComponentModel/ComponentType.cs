using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;

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

    /// <summary>
    /// Finds all component types in the current AppDomain that match the given <paramref name="predicate"/>, or all
    /// component types if the predicate is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// The type passed to the predicate is already known to be a class that derives from <see cref="Component"/>.
    /// </remarks>
    /// <param name="predicate">A function to filter component types, or <c>null</c> to include all component types.</param>
    /// <returns>A collection of <see cref="IComponentType"/> instances that match the given predicate.</returns>
    public static IEnumerable<IComponentType> FindAll(Func<Type, bool>? predicate = null)
    {
        var types = AppDomain.CurrentDomain.GetTypes().Where(t => t.IsClass && t.IsAssignableTo<Component>());

        if (predicate is not null)
        {
            types = types.Where(predicate);
        }

        return types.Select(t => For(t));
    }

    /// <summary>
    /// Finds all component types in the current AppDomain that match the given <paramref name="name"/>.
    /// </summary>
    /// <remarks>
    /// The name must be a case-sensitive match for either the type's <see cref="MemberInfo.Name"/> or its
    /// <see cref="Type.FullName"/>. Comparison is done using <see cref="StringComparison.Ordinal"/>.
    /// </remarks>
    /// <param name="name">The name of the component types to find.</param>
    /// <returns>A collection of <see cref="IComponentType"/> instances that match the given name.</returns>
    public static IEnumerable<IComponentType> FindAll(string name)
    {
        return FindAll(t =>
        {
            // don't use the current culture, otherwise Turkish developers won't notice problems with 'i' but others will

            if (string.Equals(t.Name, name, StringComparison.Ordinal))
            {
                return true;
            }

            if (t.FullName is not null)
            {
                if (string.Equals(t.FullName, name, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        });
    }

    /// <summary>
    /// Finds the first component type in the current AppDomain that matches the given <paramref name="name"/>.
    /// </summary>
    /// <remarks>
    /// The name must be a case-sensitive match for either the type's <see cref="MemberInfo.Name"/> or its
    /// <see cref="Type.FullName"/>. Comparison is done using <see cref="StringComparison.Ordinal"/>.
    /// <br/>
    /// If multiple component types match the given name, an exception is thrown.
    /// </remarks>
    /// <param name="name">The name of the component type to find.</param>
    /// <returns>The <see cref="IComponentType"/> that matches the given name, or <c>null</c> if no such component type exists.</returns>
    public static IComponentType? Find(string name)
    {
        return FindAll(name).SingleOrDefault(); // make sure we don't return multiple types with the same name, which would be ambiguous
    }
}

internal class ComponentType<TComponent> : IComponentType<TComponent>
    where TComponent : Component
{
    public static ComponentType<TComponent> Instance { get; } = new();

    public Type Type { get; } = typeof(TComponent);
}
