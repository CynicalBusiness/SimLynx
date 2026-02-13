
using Autofac;

namespace SimLynx.Core.Components;

/// <summary>
/// Generalized definition of a component that can be registered to a <see cref="Prototype"/>.
/// </summary>
public interface IComponentDef
{

    /// <summary>
    /// The component type metadata for the component defined by this definition.
    /// </summary>
    public IComponentType ComponentType { get; }

    /// <summary>
    /// Creates a component based on this definition for the provided prototype.
    /// </summary>
    /// <param name="prototype">The prototype to which the component will be attached.</param>
    /// <param name="scope">The injection scope to create the component in</param>
    /// <returns>A new component instance.</returns>
    public Component Create(Prototype prototype, ILifetimeScope scope);

}
