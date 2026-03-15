
using System.Collections.Generic;
using Autofac;
using SimLynx.Core.Phasing;

namespace SimLynx.Design;

/// <summary>
/// Phase manager for the <see cref="DesignPhase"/>.
/// </summary>
public class DesignPhaseManager(
    ILifetimeScope scope,
    IEnumerable<IDesignRegistrationProvider> designProviders)
    : PhaseManager<DesignPhase>(scope)
{

    /// <inheritdoc/>
    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        base.ConfigureContainer(builder);

        foreach (var provider in designProviders)
        {
            provider.ConfigureDesign(builder);
        }
    }

}
