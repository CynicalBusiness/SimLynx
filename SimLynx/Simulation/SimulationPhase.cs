using SimLynx.Core.Phasing;

namespace SimLynx.Simulation;

/// <summary>
/// Host for the main simulation phase of SimLynx, which is responsible for handling all simulation within SimLynx.
/// </summary>
public class SimulationPhase : Phase
{
    /// <summary>
    /// ID of the phase, used for keying dependencies.
    /// </summary>
    public static readonly Symbol PhaseId = nameof(SimulationPhase);
}
