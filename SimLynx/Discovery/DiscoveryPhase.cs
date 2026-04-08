using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using SimLynx.Core.Phasing;
using SimLynx.Design;

namespace SimLynx.Discovery;

/// <summary>
/// Host for the discovery phase of SimLynx, which is responsible for discovering content and preparing it for the
/// design phase.
/// </summary>
public class DiscoveryPhase : Phase
{
    /// <summary>
    /// ID of the phase, used for keying dependencies.
    /// </summary>
    public static readonly Symbol PhaseId = nameof(DiscoveryPhase);
    private readonly IEnumerable<IDiscoveryService> discoveryServices;

    /// <inheritdoc cref="DiscoveryPhase"/>
    public DiscoveryPhase(IEnumerable<IDiscoveryService> discoveryServices)
    {
        this.discoveryServices = discoveryServices;
        NextPhaseId = DesignPhase.PhaseId;
    }

    /// <inheritdoc/>
    protected override Task RunPhase(CancellationToken cancellationToken)
    {
        return Task.WhenAll(discoveryServices.Select(s => s.RunDiscoveryAsync(cancellationToken)));
    }

    /// <summary>
    /// Builder for the <see cref="DiscoveryPhase"/>.
    /// </summary>
    public class Builder(
        ILogger<Builder> logger,
        ILifetimeScope scope,
        IEnumerable<IDiscoveryRegistrationProvider> discoveryProviders
    ) : PhaseBuilder<DiscoveryPhase>(logger, scope)
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
}
