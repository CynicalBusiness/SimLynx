
using Autofac;
using SimLynx.Core;
using SimLynx.Core.Components;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SimLynx;

/// <summary>
/// Defines the components that make up an entity.
/// </summary>
/// <remarks>
/// An entity's prototype is the collection of components that define it.
/// </remarks>
public class Prototype : IHaveSymbolicName
{

    /// <param name="name">The name of the prototype, which is used for registration.</param>
    /// <param name="scope">The injection scope of this prototype</param>
    /// <param name="componentDefs">The component definitions that make up the prototype.</param>
    public Prototype(
        Symbol name,
        ILifetimeScope scope,
        IEnumerable<IComponentDef> componentDefs)
    {
        Name = name;
        Components = componentDefs.ToImmutableDictionary(
            def => def,
            def => def.Create(this, scope));
    }

    /// <summary>
    /// The components that make up this prototype, mapped by their definitions.
    /// </summary>
    public IReadOnlyDictionary<IComponentDef, Component> Components { get; }

    /// <summary>
    /// The name of the prototype, which is used for registration.
    /// </summary>
    public Symbol Name { get; }

    /// <summary>
    /// Initializes the prototype and all of its components.
    /// </summary>
    public void Init()
    {
        foreach (var component in Components.Values)
        {
            component.Init();
        }
    }

}
