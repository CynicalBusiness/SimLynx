namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// A prototype for an <see cref="Entity"/>.
/// </summary>
public class EntityPrototype : ComponentPrototype
{
    /// <inheritdoc cref="ComponentPrototype.ComponentType"/>
    public new IComponentType<Entity> ComponentType
    {
        get => (IComponentType<Entity>)base.ComponentType;
        set => base.ComponentType = value;
    }

    /// <inheritdoc/>
    protected override IComponentType<Entity> DefaultComponentType => ComponentTypes.For<Entity>();
}
