using SimLynx.Simulation.ComponentModel.Prototyping;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// A builder-like type that is passed to components, allowing them to configure themselves.
/// </summary>
public interface IComponentBuildContext
{
    /// <summary>
    /// The prototype of the component being built.
    /// </summary>
    public IComponentPrototype Prototype { get; }

    /// <summary>
    /// The name of the component being built, if any.
    /// </summary>
    public Symbol Name { get; }

    /// <summary>
    /// Registers an instance state on a component.
    /// </summary>
    /// <remarks>
    /// The state is identified by its type on this particular component. Two components requesting the same state type
    /// will receive separate state handles, but the same component requesting the same state type multiple times will
    /// receive the same state handle.
    /// </remarks>
    /// <typeparam name="TValue">The type of the state value.</typeparam>
    /// <returns>A handle to the registered state.</returns>
    public IInstanceState<TValue> RegisterState<TValue>()
        where TValue : notnull;
}
