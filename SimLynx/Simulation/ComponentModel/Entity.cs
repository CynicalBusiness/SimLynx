namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// A special type of component that represents the "root" of a component hierarchy, and serves as a container for other
/// components.
/// </summary>
/// <remarks>
/// An <see cref="Entity"/> is a special type of <see cref="Component"/>, but is still a valid component for other
/// entities, allowing creating "sub-entities" that are part of a parent entity.
/// </remarks>
public class Entity(EntityPrototype prototype) : Component(prototype)
{
    /// <inheritdoc/>
    public override EntityPrototype Prototype { get; } = prototype;
}
