using SimLynx.Core.Phasing;
using SimLynx.Simulation;

namespace SimLynx.Design;

/// <summary>
/// Host for the design phase of SimLynx, which is responsible for building simulation structures from loaded
/// content.
/// </summary>
public class DesignPhase : Phase
{
    /// <summary>
    /// ID of the phase, used for keying dependencies.
    /// </summary>
    public static readonly string PhaseId = nameof(DesignPhase);

    /// <inheritdoc cref="DesignPhase"/>
    public DesignPhase()
    {
        NextPhaseId = SimulationPhase.PhaseId;
    }
}
