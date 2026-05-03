using System;
using SimLynx.Core;
using SimLynx.Core.Phasing;

namespace SimLynx.Simulation;

/// <summary>
/// Host for the main simulation phase of SimLynx, which is responsible for handling all simulation within SimLynx.
/// </summary>
public class SimulationPhase : Phase, IDisposable
{
    /// <summary>
    /// Name of the metronome used for the simulation phase's internal timing.
    /// </summary>
    public const string METRONOME_NAME = "Simulation";

    /// <summary>
    /// The default target rate at which the simulation will update.
    /// </summary>
    /// <remarks>
    /// This is currently at 20 ticks per second.
    /// </remarks>
    public static TimeSpan DefaultTickRate { get; set; } = TimeSpan.FromSeconds(1.0 / 20.0); // 20 ticks per second

    /// <summary>
    /// ID of the phase, used for keying dependencies.
    /// </summary>
    public static readonly Symbol PhaseId = nameof(SimulationPhase);

    /// <summary>
    /// The metronome used for the simulation phase's internal timing.
    /// </summary>
    public required Metronome Metronome { get; init; }

    /// <inheritdoc/>
    public void Dispose()
    {
        Metronome.Dispose();
    }
}
