using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using SimLynx.Core.Phasing;

namespace SimLynx.Design;

/// <summary>
/// Host for the design phase of SimLynx, which is responsible for building simulation structures from loaded
/// content.
/// </summary>
public class DesignPhase(IEnumerable<IDesignService> designServices, IIndex<Symbol, IPhaseManager> phaseManagers)
    : Phase(phaseManagers)
{
    /// <summary>
    /// ID of the phase, used for keying dependencies.
    /// </summary>
    public static readonly Symbol PhaseId = nameof(DesignPhase);

    /// <inheritdoc/>
    protected override Task RunAsync(CancellationToken cancellationToken)
    {
        return Task.WhenAll(designServices.Select(s => s.RunDesignAsync(cancellationToken)));
    }
}
