using Autofac.Builder;

namespace SimLynx.Simulation;

/// <summary>
/// Extensions related to the simulation phase.
/// </summary>
public static class SimulationExtensions
{
    extension<TLimit, TActivatorData, TRegistrationStyle>(
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder
    )
    {
        /// <summary>
        /// Configures the registration to be a simulation instance, meaning it will be a single instance for each
        /// simulation phase.
        /// </summary>
        /// <returns>The builder for further configuration.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> SimulationInstance()
        {
            return builder.InstancePerOwned<SimulationPhase>();
        }
    }
}
