
using System;
using Autofac;

namespace SimLynx.Core.States;

/// <summary>
/// Resolver that dynamically resolves state types from its current scope.
/// </summary>
public class StateTypeResolver(ILifetimeScope scope)
{

    /// <summary>
    /// Dynamically resolves a state type from the current scope.
    /// </summary>
    /// <param name="type">The type of the state to resolve.</param>
    /// <returns>The resolved state type.</returns>
    public IStateType Resolve(Type type)
    {
        return (IStateType)scope.Resolve(typeof(StateType<>).MakeGenericType(type));
    }

    /// <summary>
    /// Resolves a state type from the current scope of the given type.
    /// </summary>
    /// <typeparam name="TState">The state type to resolve</typeparam>
    /// <returns>The resolved state type.</returns>
    public StateType<TState> Resolve<TState>()
        where TState : IState
    {
        return scope.Resolve<StateType<TState>>();
    }

}
