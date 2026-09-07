using System;
using System.Collections.Generic;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Resolver that resolves <see cref="IComponentTypeConfig"/>s for component types.
/// </summary>
public interface IComponentTypeResolver
{
    /// <summary>
    /// Dynamically resolves <see cref="IComponentTypeConfig"/> instances for the given component type.
    /// </summary>
    /// <remarks>
    /// The configs are returned for the base-most type (i.e. <see cref="Component"/>) first, in registration order,
    /// then so on for each more derived type, until the specified component type is reached.
    /// </remarks>
    /// <param name="componentType">The type of the component.</param>
    /// <returns>A collection of <see cref="IComponentTypeConfig"/> instances for the given component type.</returns>
    public IEnumerable<IComponentTypeConfig> GetConfigs(Type componentType);

    /// <summary>
    /// Resolves <see cref="IComponentTypeConfig{TComponent}"/> instances for the given component type.
    /// </summary>
    /// <remarks>
    /// The configs are returned for the base-most type (i.e. <see cref="Component"/>) first, in registration order,
    /// then so on for each more derived type, until the specified component type is reached.
    /// </remarks>
    /// <typeparam name="TComponent">The type of the component.</typeparam>
    /// <returns>A collection of <see cref="IComponentTypeConfig{TComponent}"/> instances for the given component type.</returns>
    public IEnumerable<IComponentTypeConfig<TComponent>> GetConfigs<TComponent>()
        where TComponent : Component;
}
