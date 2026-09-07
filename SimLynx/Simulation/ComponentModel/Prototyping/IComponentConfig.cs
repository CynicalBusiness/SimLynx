using SimLynx.Design.Prototyping;

namespace SimLynx.Simulation.ComponentModel.Prototyping;

/// <summary>
/// Generalized config which attaches a component to a <typeparamref name="TSubject"/> component.
/// </summary>
/// <typeparam name="TSubject">The type of subject (parent) component</typeparam>
public interface IComponentConfig<TSubject> : IPrototypeConfig<TSubject>
    where TSubject : Component
{
    /// <summary>
    /// The prototype of the component being attached.
    /// </summary>
    public IComponentPrototype<Component>? ComponentPrototype { get; }
}

/// <summary>
/// A config which attaches a <typeparamref name="TComponent"/> component to a <typeparamref name="TSubject"/>.
/// </summary>
/// <typeparam name="TSubject">The type of subject (parent) component</typeparam>
/// <typeparam name="TComponent">The type of component to attach</typeparam>
public interface IComponentConfig<TSubject, TComponent> : IComponentConfig<TSubject>
    where TSubject : Component
    where TComponent : Component
{
    /// <inheritdoc cref="IComponentConfig{TSubject}.ComponentPrototype"/>
    public new ComponentPrototype<TComponent>? ComponentPrototype { get; }

    IComponentPrototype<Component>? IComponentConfig<TSubject>.ComponentPrototype => ComponentPrototype;
}
