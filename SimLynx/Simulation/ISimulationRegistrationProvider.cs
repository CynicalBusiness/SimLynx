
using Autofac;

namespace SimLynx.Simulation;

/// <summary>
/// Provider of simulation-phase registrations, used to configure the simulation phase's container.
/// </summary>
public interface ISimulationRegistrationProvider
{
    /// <summary>
    /// Configures the simulation phase container.
    /// </summary>
    /// <param name="builder">The container builder used to configure the simulation phase container.</param>
    public void ConfigureSimulation(ContainerBuilder builder);
}
