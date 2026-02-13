
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimLynx.Core.Entities;

/// <summary>
/// Builder class for constructing components.
/// </summary>
/// <remarks>
/// This class will generally be passed to component constructors to allow them to configure their state and other
/// properties.
/// </remarks>
public class EntityBuilder(StateTypeResolver stateTypeResolver)
{
    private static Symbol GetStateId(Type stateType)
    {
        return stateType.ToSymbol();
    }

    private readonly Dictionary<Symbol, IStateBuilder> states = [];
    private readonly List<Symbol> stateIds = [];

    /// <summary>
    /// Registers a state to the entity being built, returning its handle. The returned handle can then be used
    /// to reference the state on entities.
    /// </summary>
    /// <typeparam name="TState">The type of state</typeparam>
    /// <param name="configure">An optional configuration action for the state registration</param>
    /// <returns>The state handle</returns>
    public StateHandle<TState> RegisterState<TState>(Action<StateBuilder<TState>>? configure = null)
        where TState : IState
    {
        var stateId = GetStateId(typeof(TState));

        StateBuilder<TState> builder;
        if (states.TryGetValue(stateId, out var genericBuilder))
        {
            builder = (StateBuilder<TState>)genericBuilder;
        }
        else
        {
            builder = new(stateTypeResolver.Resolve<TState>());
            states.Add(stateId, builder);
            stateIds.Add(stateId);
        }

        configure?.Invoke(builder);
        return new(stateId);
    }

    /// <inheritdoc />
    public StateHandle<TState> RegisterState<TState>()
        where TState : IState
    {
        return RegisterState<TState>(null);
    }

    /// <summary>
    /// Enumerable of states registered so far.
    /// </summary>
    /// <remarks>
    /// As this builder is not designed to be used concurrently nor is it immutable, this enumerable has undefined
    /// behavior if states are registered during enumeration.
    /// </remarks>
    public IEnumerable<KeyValuePair<Symbol, IStateBuilder>> States => stateIds
        .Select(id => new KeyValuePair<Symbol, IStateBuilder>(id, states[id]));

}
