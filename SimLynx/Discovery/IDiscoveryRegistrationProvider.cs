
using Autofac;

namespace SimLynx.Discovery;

/// <summary>
/// Provider of discovery-phase registrations, used to configure the discovery phase's container.
/// </summary>
public interface IDiscoveryRegistrationProvider
{
    /// <summary>
    /// Configures the discovery phase container.
    /// </summary>
    /// <param name="builder">The container builder used to configure the discovery phase container.</param>
    public void ConfigureDiscovery(ContainerBuilder builder);
}
