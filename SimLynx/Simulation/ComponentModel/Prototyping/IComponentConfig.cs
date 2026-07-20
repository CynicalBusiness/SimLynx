using SimLynx.Core.Prototyping;

namespace SimLynx.Simulation.ComponentModel.Prototyping;

/// <summary>
/// Generalized interface for a component config for a <see cref="Component"/> subject.
/// </summary>
/// <typeparam name="TSubject">The type of subject (parent) component</typeparam>
public interface IComponentConfig<TSubject> : IPrototypeConfig<TSubject>
    where TSubject : Component
{
    /// <summary>
    /// The prototype of the component being attached.
    /// </summary>
    public IComponentPrototype<Component> ComponentPrototype { get; }
}

/// <summary>
/// Interface for a component config which attaches a <typeparamref name="TComponent"/> to a <typeparamref name="TSubject"/> component.
/// </summary>
/// <typeparam name="TSubject">The type of subject (parent) component</typeparam>
/// <typeparam name="TComponent">The type of component to attach</typeparam>
public interface IComponentConfig<TSubject, TComponent> : IComponentConfig<TSubject>
    where TSubject : Component
    where TComponent : Component
{
    /// <inheritdoc cref="IComponentConfig{TSubject}.ComponentPrototype"/>
    public new ComponentPrototype<TComponent> ComponentPrototype { get; }

    IComponentPrototype<Component> IComponentConfig<TSubject>.ComponentPrototype => ComponentPrototype;
}
