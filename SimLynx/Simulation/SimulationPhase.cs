using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using SimLynx.Core.Phasing;

namespace SimLynx.Simulation;

/// <summary>
/// Host for the main simulation phase of SimLynx, which is responsible for handling all simulation within SimLynx.
/// </summary>
public class SimulationPhase(IEnumerable<ISimulationService> simulationServices) : Phase
{
    /// <summary>
    /// ID of the phase, used for keying dependencies.
    /// </summary>
    public static readonly Symbol PhaseId = nameof(SimulationPhase);

    /// <inheritdoc/>
    protected override Task RunPhase(CancellationToken cancellationToken)
    {
        return Task.WhenAll(simulationServices.Select(s => s.RunSimulationAsync(cancellationToken)));
    }

    /// <summary>
    /// Builder for the <see cref="SimulationPhase"/>.
    /// </summary>
    public class Builder(ILifetimeScope scope, IEnumerable<ISimulationRegistrationProvider> registrationProviders)
        : PhaseBuilder<SimulationPhase>(scope)
    {
        /// <inheritdoc/>
        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);

            foreach (var provider in registrationProviders)
            {
                provider.ConfigureSimulation(builder);
            }
        }
    }
}
