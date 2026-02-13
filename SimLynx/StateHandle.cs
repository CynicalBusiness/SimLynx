
namespace SimLynx;

/// <summary>
/// Handle for a state that is registered to an <see cref="Prototype"/>.
/// </summary>
/// <typeparam name="TState">The type of state</typeparam>
public readonly struct StateHandle<TState>
    where TState : IState
{
    internal StateHandle(Symbol id)
    {
        Id = id;
    }

    internal Symbol Id { get; init; }
}
