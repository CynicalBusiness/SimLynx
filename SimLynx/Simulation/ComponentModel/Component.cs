using SimLynx.Core.Prototyping;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Base class for all components.
/// <br/>
/// Components are the fundamental building blocks of SimLynx's simulation, being the primary container for logic
/// and acting as an interface for its underlying data.
/// </summary>
/// <remarks>
/// Of note: do not confuse SimLynx components with that of components in an Entity-Component-System (ECS) architecture.
/// Components are much broader in scope than ECS components, with component *instances* being more analogous to
/// their ECS counterparts. SimLynx components can somewhat be compared to narrow-scoped systems.
/// </remarks>
/// <param name="ctx">The builder passed to the component for configuration.</param>
public abstract class Component(IComponentBuildContext ctx) : IPrototypeSubject
{
    /// <summary>
    /// The prototype of this component.
    /// </summary>
    public IComponentPrototype Prototype { get; } = ctx.Prototype;
    IPrototype IPrototypeSubject.Prototype => Prototype;

    /// <summary>
    /// The name of this component, if any.
    /// </summary>
    /// <remarks>
    /// This property will be equal to <see cref="Symbol.Empty"/> if this component was not assigned a name.
    /// </remarks>
    public Symbol Name { get; } = ctx.Name;
}
