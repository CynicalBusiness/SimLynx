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
public abstract class Component(ComponentPrototype prototype)
{
    /// <summary>
    /// The prototype used to create this component.
    /// </summary>
    public virtual ComponentPrototype Prototype { get; } = prototype;
}
