using System;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// A special-case component designed to be the root of a component hierarchy, collectively referred to as an "entity".
/// </summary>
/// <remarks>
/// The instance of an entity component can be used to "identify" a particular instance and serves as a "root handle"
/// to that instance.
/// <br/>
/// While the root component of a hierarchy must be an entity, the reverse is not necessarily true: an entity <em>is
/// a component</em> like any other and can be used as a child of another component. Other components can reference
/// these "owned" entities like any other entity, but said entity is permanently attached to, and is destroyed with,
/// its parent component.
/// <br/>
/// An entity component may be subclassed to broadly group instances by their purpose (eg. Actor, Item, Effect,
/// etc.).
/// </remarks>
public class Entity(IComponentContextInfo ctx) : Component(ctx)
{
    /// <summary>
    /// The name of the state which stores the unique identifier for this entity.
    /// </summary>
    protected static readonly Symbol EntityIdStateName = Symbol.For(nameof(entityId));

    /// <summary>
    /// State for an entity's unique identifier.
    /// </summary>
    protected readonly InstanceState<Guid> entityId;
}
