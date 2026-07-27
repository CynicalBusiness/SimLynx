using System;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Scope tag used for lifetime scopes that are specific to a phase.
/// </summary>
/// <param name="PhaseType">The type of the phase.</param>
public record PhaseScopeTag(Type PhaseType)
{
    /// <summary>
    /// Creates a new <see cref="PhaseScopeTag"/> for the given phase type.
    /// </summary>
    /// <typeparam name="TPhase">The type of the phase.</typeparam>
    /// <returns>A new <see cref="PhaseScopeTag"/> instance.</returns>
    public static PhaseScopeTag For<TPhase>()
        where TPhase : Phase
    {
        return new PhaseScopeTag(typeof(TPhase));
    }
}
