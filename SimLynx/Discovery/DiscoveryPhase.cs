using SimLynx.Core.Phasing;
using SimLynx.Design;

namespace SimLynx.Discovery;

/// <summary>
/// Host for the discovery phase of SimLynx, which is responsible for discovering content and preparing it for the
/// design phase.
/// </summary>
public class DiscoveryPhase : Phase
{
    /// <summary>
    /// ID of the phase, used for keying dependencies.
    /// </summary>
    public static readonly Symbol PhaseId = nameof(DiscoveryPhase);

    /// <inheritdoc cref="DiscoveryPhase"/>
    protected DiscoveryPhase()
    {
        NextPhaseId = DesignPhase.PhaseId;
    }
}
