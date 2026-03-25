using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using SimLynx.Core.Phasing;

namespace SimLynx.Simulation;

/// <summary>
/// Host for the main simulation phase of SimLynx, which is responsible for handling all simulation within SimLynx.
/// </summary>
public class SimulationPhase(
    IEnumerable<ISimulationService> simulationServices,
    IIndex<Symbol, IPhaseManager> phaseManagers
) : Phase(phaseManagers)
{
    /// <summary>
    /// ID of the phase, used for keying dependencies.
    /// </summary>
    public static readonly Symbol PhaseId = nameof(SimulationPhase);

    /// <inheritdoc/>
    protected override Task RunAsync(CancellationToken cancellationToken)
    {
        return Task.WhenAll(simulationServices.Select(s => s.RunSimulationAsync(cancellationToken)));
    }
}
