
using SimLynx.Core.States;

namespace SimLynx;

/// <summary>
/// Builder for configuring state registrations for a particular state type.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <param name="stateType">The state type to configure.</param>
public class StateBuilder<TState>(
    StateType<TState> stateType)
        : IStateBuilder
        where TState : IState
{
    private event ConfigureDefaultsFunc? ConfigureDefault;

    /// <summary>
    /// Delegate type for configuring default state values by ref.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    public delegate void ConfigureDefaultsFunc(ref TState state);

    /// <inheritdoc cref="IStateBuilder.StateType"/>
    public StateType<TState> StateType { get; } = stateType;
    IStateType IStateBuilder.StateType => StateType;

    /// <summary>
    /// Configures initial state values using a delegate that takes the default value (or value configured by another
    /// component) and returns the configured value.
    /// </summary>
    /// <param name="defaultFunc">The delegate used to configure the initial state values.</param>
    /// <returns>The state registration builder.</returns>
    public StateBuilder<TState> Initially(ConfigureDefaultsFunc defaultFunc)
    {
        ConfigureDefault += defaultFunc;
        return this;
    }

    internal TState Configure()
    {
        var state = StateType.CreateDefault();
        ConfigureDefault?.Invoke(ref state);
        return state;
    }
}
