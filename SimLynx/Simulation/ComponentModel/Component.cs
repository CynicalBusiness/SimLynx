using SimLynx.Design.Prototyping;
using SimLynx.Simulation.ComponentModel.Prototyping;

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
public abstract class Component(IComponentContextInfo ctx) : IPrototypeSubject
{
    /// <summary>
    /// The context of this component, which provides information about the component within its entity.
    /// </summary>
    protected IComponentContextInfo Context { get; } = ctx;

    /// <summary>
    /// The prototype of this component.
    /// </summary>
    public IComponentPrototype Prototype => Context.Prototype;
    IPrototype IPrototypeSubject.Prototype => Prototype;
}
