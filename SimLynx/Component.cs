
using SimLynx.Core.Entities;

namespace SimLynx;

/// <summary>
/// The fundamental building block of SimLynx, used to assemble <see cref="Prototype">Prototypes</see> and define
/// states/behaviors for their resulting entities.
/// </summary>
/// <remarks>
/// An <see cref="Prototype"/> is defined by its Component(s), each being able to configure one or more properties of
/// the underlying entity. Components may provide initial states values, configure behaviors, and/or define
/// operations which act as an API for interacting with instances of their entity. <br/>
/// <br/>
/// Any descendant type may be configured by a "definition" that allows external configurations to configure
/// the component. Public properties with either a setter or initializer are considered configurable.
/// </remarks>
public abstract class Component(ComponentBuilder builder)
{
    /// <summary>
    /// Metadata for the prototype.
    /// </summary>
    protected readonly StateHandle<EntityMetadata> metadata = builder.Entity.RegisterState<EntityMetadata>();

    /// <summary>
    /// The prototype that this component is part of.
    /// </summary>
    public Prototype Prototype { get; } = builder.Prototype;

    /// <summary>
    /// Allows for any additional initialization of the component
    /// </summary>
    /// <remarks>
    /// This method is called after <em>all</em> components have been constructed and defs applied, called in the same
    /// order the components were added to the prototype. This allows for components to reference each other, even those
    /// which may have been defined later.
    /// <br/>
    /// Override this method to define additional behavior, making sure to call <c>base.Init()</c> to ensure the base
    /// initialization is performed.
    /// </remarks>
    protected internal virtual void Init()
    {
        // no-op by default
    }

}
