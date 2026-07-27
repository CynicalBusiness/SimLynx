using System;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Hook for initialization of a phase, which occurs once for each phase before it is configured and run.
/// </summary>
/// <param name="PhaseType">The type of the phase being initialized.</param>
public record OnPhaseInit(Type PhaseType);

/// <summary>
/// Hook for initialization of a <typeparamref name="TPhase"/>, which occurs once for each phase before it is configured
/// and run.
/// </summary>
/// <typeparam name="TPhase">The type of the phase being initialized.</typeparam>
public record OnPhaseInit<TPhase>() : OnPhaseInit(typeof(TPhase));
