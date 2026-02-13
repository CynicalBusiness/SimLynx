
using System;
using SimLynx.Core.States;

namespace SimLynx;

/// <summary>
/// Meta-type containing information about a state type, such as its static configuration or instantiation logic.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
public class StateType<TState>
    : IStateType
    where TState : IState
{
    /// <summary>
    /// Implicit conversion to <see cref="System.Type"/> via <see cref="StateType{TState}.Type"/>
    /// </summary>
    /// <param name="stateType">The state type to convert.</param>
    public static implicit operator Type(StateType<TState> stateType) => stateType.Type;

    private StateType()
    {
    }

    /// <inheritdoc />
    public Type Type => typeof(TState);

    /// <summary>
    /// Creates a default instance of the state.
    /// </summary>
    /// <remarks>
    /// Note that the "default instance" of a state is not the same as the C# <c>default(TState)</c>, as a full
    /// instance will be provided in the case of reference (i.e. "managed") types rather than <c>null</c>.
    /// </remarks>
    /// <returns>The default instance of the state.</returns>
    public TState CreateDefault()
    {
        throw new NotImplementedException();
    }
}
