
using System;

namespace SimLynx.Core.States;

/// <summary>
/// Generalized builder interface for configuring state registrations.
/// </summary>
public interface IStateBuilder
{

    /// <summary>
    /// The type of state being built.
    /// </summary>
    public IStateType StateType { get; }

}
