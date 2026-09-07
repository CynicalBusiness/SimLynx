using SimLynx.Design.Prototyping;

namespace SimLynx.Simulation.ComponentModel.Prototyping;

/// <summary>
/// Base class for a prototype of a <see cref="Component"/>.
/// </summary>
public class ComponentPrototype<TComponent>(Symbol id, PrototypeContext ctx)
    : Prototype<TComponent>(id, ctx),
        IComponentPrototype<TComponent>
    where TComponent : Component
{
    /// <inheritdoc/>
    public override bool IsAbstract
    {
        get => base.IsAbstract || typeof(TComponent).IsAbstract;
        init => base.IsAbstract = value;
    }
}
