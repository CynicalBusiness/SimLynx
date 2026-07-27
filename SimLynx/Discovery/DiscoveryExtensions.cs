using Autofac.Builder;
using SimLynx.Core.Phasing;

namespace SimLynx.Discovery;

/// <summary>
/// Extensions related to the discovery phase.
/// </summary>
public static class DiscoveryExtensions
{
    extension<TLimit, TActivatorData, TRegistrationStyle>(
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder
    )
    {
        /// <summary>
        /// Configures the registration to be a discovery instance, meaning it will be a single instance for each
        /// discovery phase.
        /// </summary>
        /// <returns>The builder for further configuration.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> DiscoveryInstance()
        {
            return builder.InstancePerPhase(typeof(DiscoveryPhase));
        }
    }
}
