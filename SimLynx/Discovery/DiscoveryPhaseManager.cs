using System.Collections.Generic;
using Autofac;
using SimLynx.Core.Phasing;

namespace SimLynx.Discovery;

/// <summary>
/// Host for the discovery phase of SimLynx, which is responsible for discovering content and preparing it for the
/// design phase.
/// </summary>
public class DiscoveryPhaseManager(ILifetimeScope scope, IEnumerable<IDiscoveryRegistrationProvider> discoveryProviders)
    : PhaseManager<DiscoveryPhase>(scope)
{
    /// <inheritdoc/>
    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        base.ConfigureContainer(builder);

        foreach (var provider in discoveryProviders)
        {
            provider.ConfigureDiscovery(builder);
        }
    }
}
