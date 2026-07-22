using SimLynx.Design.Prototyping;

namespace SimLynx.Simulation.ComponentModel.Prototyping;

/// <summary>
/// Base class for a prototype of a <see cref="Component"/>.
/// </summary>
public class ComponentPrototype<TComponent>(Symbol id, PrototypeContext<TComponent> ctx)
    : Prototype<TComponent>(id, ctx),
        IComponentPrototype<TComponent>
    where TComponent : Component
{
    /// <summary>
    /// The type of component represented by this prototype.
    /// </summary>
    public virtual IComponentType ComponentType { get; } = ComponentTypes.For<TComponent>();

    /// <inheritdoc/>
    public override bool IsAbstract
    {
        get => base.IsAbstract || ComponentType.IsAbstract;
        init => base.IsAbstract = value;
    }
}
