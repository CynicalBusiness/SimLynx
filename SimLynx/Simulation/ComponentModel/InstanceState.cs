using System;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Handle to a particular type of state on a component's instance.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public readonly struct InstanceState<TValue>
    where TValue : notnull
{
    /// <summary>
    /// The key of the state type and tag.
    /// </summary>
    public readonly TypeKey Key;

    /// <summary>
    /// The type of the state value.
    /// </summary>
    public Type Type => Key.Type;

    /// <summary>
    /// The tag (name) of the state.
    /// </summary>
    public Symbol Tag => Key.Id;
}
