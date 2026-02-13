
using System;

namespace SimLynx.Core.States;

/// <summary>
/// Generalized interface for state defs.
/// </summary>
public interface IStateType
{

    /// <summary>
    /// Underlying system type of the state
    /// </summary>
    public Type Type { get; }

}
