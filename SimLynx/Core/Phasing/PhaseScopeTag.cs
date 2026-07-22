using System;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Scope tag used for lifetime scopes that are specific to a phase.
/// </summary>
/// <param name="PhaseType">The type of the phase.</param>
/// <param name="PhaseId">The identifier of the phase.</param>
public record PhaseScopeTag(Type PhaseType, string PhaseId)
{
    /// <summary>
    /// Creates a new <see cref="PhaseScopeTag"/> for the given phase type and ID.
    /// </summary>
    /// <typeparam name="TPhase">The type of the phase.</typeparam>
    /// <param name="phaseId">The identifier of the phase.</param>
    /// <returns>A new <see cref="PhaseScopeTag"/> instance.</returns>
    public static PhaseScopeTag For<TPhase>(string phaseId)
        where TPhase : Phase
    {
        return new PhaseScopeTag(typeof(TPhase), phaseId);
    }
}
