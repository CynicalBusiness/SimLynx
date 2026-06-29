using SimLynx.Core.Prototyping;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Base class for a prototype of a <see cref="Component"/>.
/// </summary>
public class ComponentPrototype<TComponent>(Symbol id, Autofac.IComponentContext ctx)
    : Prototype<TComponent>(id, ctx),
        IComponentPrototype
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
