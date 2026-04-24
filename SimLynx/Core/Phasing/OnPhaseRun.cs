using System;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Hook for each time a phase is run.
/// </summary>
/// <param name="PhaseType">The type of the phase being run.</param>
/// <param name="Phase">The instance of the phase being run.</param>
public record OnPhaseRun(Type PhaseType, IPhase Phase);

/// <summary>
/// Hook for each time a <typeparamref name="TPhase"/> is run.
/// </summary>
/// <typeparam name="TPhase">The type of the phase being run.</typeparam>
/// <param name="Phase">The instance of the phase being run.</param>
public record OnPhaseRun<TPhase>(TPhase Phase) : OnPhaseRun(typeof(TPhase), Phase)
    where TPhase : IPhase
{
    /// <inheritdoc cref="OnPhaseRun.Phase"/>
    public new TPhase Phase { get; } = Phase;
}
