
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using SimLynx.Core.Phasing;

namespace SimLynx.Discovery;

/// <summary>
/// Host for the discovery phase of SimLynx, which is responsible for discovering content and preparing it for the
/// design phase.
/// </summary>
public class DiscoveryPhase(
    IEnumerable<IDiscoveryService> discoveryServices,
    IIndex<Symbol, IPhaseManager> phaseManagers)
    : Phase(phaseManagers)
{
    /// <summary>
    /// ID of the phase, used for keying dependencies.
    /// </summary>
    public static readonly Symbol PhaseId = nameof(DiscoveryPhase);

    /// <inheritdoc/>
    protected override Task RunAsync(CancellationToken cancellationToken)
    {
        return Task.WhenAll(discoveryServices
            .Select(s => s.RunDiscoveryAsync(cancellationToken)));
    }
}
