using SimLynx.Simulation.ComponentModel.Prototyping;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Interface representing information about a component within its entity.
/// </summary>
public interface IComponentContextInfo
{
    /// <summary>
    /// The component's prototype
    /// </summary>
    public IComponentPrototype Prototype { get; }

    /// <summary>
    /// The name of this component, if any.
    /// </summary>
    public Symbol Name { get; }
}
