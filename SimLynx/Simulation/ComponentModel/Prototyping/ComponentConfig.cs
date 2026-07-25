using System;
using SimLynx.Design.Prototyping;
using SimLynx.Design.Prototyping.Blueprints;

namespace SimLynx.Simulation.ComponentModel.Prototyping;

/// <inheritdoc cref="IComponentConfig{TSubject, TComponent}"/>
public class ComponentConfig<TSubject, TComponent>(PrototypeResolver<Component> prototypeResolver, string? givenName)
    : IComponentConfig<TSubject, TComponent>
    where TSubject : Component
    where TComponent : Component
{
    /// <inheritdoc/>
    public string Name { get; } = ComponentConfigSlot.GetConfigName(ComponentTypes.For<TComponent>(), givenName);

    /// <inheritdoc/>
    public bool IsEmpty => ComponentPrototype is null;

    /// <inheritdoc/>
    public ComponentPrototype<TComponent>? ComponentPrototype { get; private set; }

    /// <inheritdoc/>
    public bool Apply(IBlueprintBuilder<TSubject> builder)
    {
        if (IsEmpty)
        {
            return false;
        }

        // TODO stub
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Clear()
    {
        ComponentPrototype = null;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        // TODO stub
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the component prototype for the component being attached, creating one if necessary.
    /// </summary>
    /// <returns>The prototype</returns>
    public ComponentPrototype<TComponent> GetComponentPrototype()
    {
        return ComponentPrototype ??=
            (ComponentPrototype<TComponent>)prototypeResolver.Resolve<TComponent>(Symbol.For(Name), false, null);
    }
}
