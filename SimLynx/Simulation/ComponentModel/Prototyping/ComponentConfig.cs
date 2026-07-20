using SimLynx.Core.Prototyping.Blueprints;

namespace SimLynx.Simulation.ComponentModel.Prototyping;

/// <inheritdoc cref="IComponentConfig{TSubject, TComponent}"/>
public class ComponentConfig<TSubject, TComponent>(IComponentPrototype<TSubject> parentPrototype, string? givenName)
    : IComponentConfig<TSubject, TComponent>
    where TSubject : Component
    where TComponent : Component
{
    /// <inheritdoc/>
    public string Name { get; } = ComponentConfigSlot.GetConfigName(ComponentTypes.For<TComponent>(), givenName);

    // TODO stub
    /// <inheritdoc/>
    public bool IsEmpty => throw new System.NotImplementedException();

    public ComponentPrototype<TComponent> ComponentPrototype => throw new System.NotImplementedException();

    public bool Apply(BlueprintBuilder<TSubject> builder)
    {
        throw new System.NotImplementedException();
    }

    public bool Clear()
    {
        throw new System.NotImplementedException();
    }
}
