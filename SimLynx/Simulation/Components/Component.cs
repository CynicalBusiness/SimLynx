namespace SimLynx.Simulation.Components;

/// <summary>
/// Base class for all components.
/// <br/>
/// Components are the fundamental building blocks of SimLynx's simulation, being the primary container for logic
/// and acting as an interface for its underlying data.
/// </summary>
/// <remarks>
/// Of note: do not confuse SimLynx components with that of components in an Entity-Component-System (ECS) architecture.
/// Components are constructed per-<em>prototype</em>, and not per-entity, nor do they hold any
/// instance data; they are more akin to a "system" in ECS, though narrower in scope.
/// </remarks>
public abstract class Component { }
