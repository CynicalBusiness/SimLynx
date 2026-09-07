using System;
using SimLynx.Simulation.ComponentModel.Prototyping;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Generalized configuration for some kind of component type.
/// </summary>
public interface IComponentTypeConfig
{
    /// <summary>
    /// The type of component this configuration is for.
    /// </summary>
    public Type ComponentType { get; }
}

/// <summary>
/// Configuration for a <typeparamref name="TComponent"/> component type.
/// </summary>
/// <typeparam name="TComponent">The type of component.</typeparam>
public interface IComponentTypeConfig<in TComponent> : IComponentTypeConfig
    where TComponent : Component
{
    /// <summary>
    /// Configures the component type.
    /// </summary>
    /// <param name="builder">The builder to configure the component type with.</param>
    public void Configure(IComponentPrototype<TComponent> builder);
}
