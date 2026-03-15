
using System.Collections.Generic;
using Autofac;
using SimLynx.Core.Phasing;

namespace SimLynx.Simulation;

/// <summary>
/// Host for the simulation phase of SimLynx, which is responsible for handling all simulation within SimLynx.
/// </summary>
public class SimulationPhaseManager(
    ILifetimeScope scope,
    IEnumerable<ISimulationRegistrationProvider> registrationProviders)
    : PhaseManager<SimulationPhase>(scope)
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
