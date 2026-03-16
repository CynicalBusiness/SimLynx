
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Semver;
using SimLynx.Core.Phasing;
using SimLynx.Design;
using SimLynx.Discovery;
using SimLynx.Discovery.Content;
using SimLynx.Simulation;

namespace SimLynx;

/// <summary>
/// Main application controller for SimLynx. Can be extended by consumer applications to provide additional
/// functionality to SimLynx.
/// </summary>
/// <remarks>
/// For most use-cases, your main application class should extend from this class.
/// <br/>
/// </remarks>
public abstract class SimLynxApp :
        IContentPackage,
        IDiscoveryRegistrationProvider,
        IDesignRegistrationProvider,
        ISimulationRegistrationProvider
{

    /// <summary>
    /// Initializes a new app instance.
    /// </summary>
    protected SimLynxApp(ILifetimeScope scope)
    {
        Scope = scope;
        Manifest = CreateContentManifest();
    }

    /// <inheritdoc/>
    public ContentPackageManifest Manifest { get; }

    /// <summary>
    /// Configuration of phases the application will run, in order.
    /// <br/>
    /// By default, this will run the Discovery, Design, then Simulation phases, but can be modified to adjust
    /// behavior, such as introducing new phases.
    /// </summary>
    protected List<Symbol> PhasePlan { get; set; } = [
        DiscoveryPhase.PhaseId,
        DesignPhase.PhaseId,
        SimulationPhase.PhaseId
    ];

    /// <summary>
    /// The Autofac scope for this application.
    /// </summary>
    protected ILifetimeScope Scope { get; }

    /// <summary>
    /// Runs the application, returning a task that represents its lifetime. The returned task should complete when the
    /// application has shut down.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel and stop the app.</param>
    /// <returns>A task that represents the application's lifetime.</returns>
    public Task RunAsync(CancellationToken cancellationToken)
    {
        if (PhasePlan.Count == 0)
        {
            throw new InvalidOperationException("Phase plan cannot be empty");
        }

        var initialPhaseId = PhasePlan[0];
        var initialPhaseManager = Scope.ResolveKeyed<IPhaseManager>(initialPhaseId);
        return initialPhaseManager.StartAndRunAsync([.. PhasePlan.Skip(1)], cancellationToken);
    }

    /// <inheritdoc/>
    public virtual void ConfigureDiscovery(ContainerBuilder builder)
    {
        // default no-op
    }

    /// <inheritdoc/>
    public virtual void ConfigureDesign(ContainerBuilder builder)
    {
        // default no-op
    }

    /// <inheritdoc/>
    public virtual void ConfigureSimulation(ContainerBuilder builder)
    {
        // default no-op
    }

    /// <summary>
    /// Gets the initial phase manager to run when the app starts. By default, this is the
    /// <see cref="DiscoveryPhaseManager"/>, but can be overridden.
    /// </summary>
    /// <returns>The initial phase manager.</returns>
    protected virtual IPhaseManager GetInitialPhaseManager()
    {
        return Scope.Resolve<DiscoveryPhaseManager>();
    }

    /// <summary>
    /// Override to customize the internal content manifest for the main app.
    /// </summary>
    /// <returns>The content package manifest.</returns>
    protected virtual ContentPackageManifest CreateContentManifest()
    {
        var thisType = GetType();
        var thisAssemblyVersion = thisType.Assembly.GetName().Version;

        return new ContentPackageManifest()
        {
            Id = thisType.Namespace ?? thisType.Name,
            Version = thisAssemblyVersion is not null
                ? SemVersion.FromVersion(thisAssemblyVersion)
                : new SemVersion(0, 0, 1)
        };
    }

}
