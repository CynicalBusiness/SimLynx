using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using SimLynx.Core.Phasing;
using SimLynx.Simulation;

namespace SimLynx.Design;

/// <summary>
/// Host for the design phase of SimLynx, which is responsible for building simulation structures from loaded
/// content.
/// </summary>
public class DesignPhase : Phase
{
    /// <summary>
    /// ID of the phase, used for keying dependencies.
    /// </summary>
    public static readonly Symbol PhaseId = nameof(DesignPhase);
    private readonly IEnumerable<IDesignService> designServices;

    /// <inheritdoc cref="DesignPhase"/>
    public DesignPhase(IEnumerable<IDesignService> designServices)
    {
        this.designServices = designServices;
        NextPhaseId = SimulationPhase.PhaseId;
    }

    /// <inheritdoc/>
    protected override Task RunPhase(CancellationToken cancellationToken)
    {
        return Task.WhenAll(designServices.Select(s => s.RunDesignAsync(cancellationToken)));
    }

    /// <summary>
    /// Builder for the <see cref="DesignPhase"/>.
    /// </summary>
    public class Builder(ILifetimeScope scope, IEnumerable<IDesignRegistrationProvider> designProviders)
        : PhaseBuilder<DesignPhase>(scope)
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
}
