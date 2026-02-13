
using SimLynx.Core.Components;
using SimLynx.Core.Entities;

namespace SimLynx;

/// <summary>
/// Configuration for a <see cref="Component"/>, intended to be passed to its constructor.
/// </summary>
/// <remarks>
/// Contains information about the component itself, its prototype, and a builder for configuring entities, among
/// others.
/// </remarks>
public class ComponentBuilder(
    Prototype prototype,
    IComponentDef def,
    EntityBuilder entityBuilder)
{
    /// <summary>
    /// The prototype that this component is being constructed for.
    /// </summary>
    public Prototype Prototype { get; } = prototype;

    /// <summary>
    /// The definition of the component being constructed.
    /// </summary>
    public IComponentDef Def { get; } = def;

    /// <summary>
    /// The builder for configuring the entity that this component is part of.
    /// </summary>
    public EntityBuilder Entity { get; } = entityBuilder;
}
