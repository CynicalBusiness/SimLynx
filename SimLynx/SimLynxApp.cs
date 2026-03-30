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
public abstract class SimLynxApp
    : IContentPackage,
        IDiscoveryRegistrationProvider,
        IDesignRegistrationProvider,
        ISimulationRegistrationProvider
{
    /// <summary>
    /// Initializes a new app instance.
    /// </summary>
    protected SimLynxApp()
    {
        Manifest = CreateContentManifest();
    }

    /// <inheritdoc/>
    public ContentPackageManifest Manifest { get; }

    /// <summary>
    /// The Autofac scope for this application.
    /// </summary>
    public required ILifetimeScope Scope { get; init; }

    /// <summary>
    /// The phase ID that the app will begin with.
    /// </summary>
    public Symbol InitialPhaseId { get; protected init; } = DiscoveryPhase.PhaseId;

    /// <summary>
    /// Runs the application, returning a task that represents its lifetime. The returned task should complete when the
    /// application has shut down.
    /// </summary>
    /// <remarks>
    /// If overriding this method, call <c>base.RunAsync</c> only when you want the app to actually start. Any pre-start
    /// logic should be before the super call, and any post-shutdown logic after it.
    /// </remarks>
    /// <param name="cancellationToken">A token to cancel and stop the app.</param>
    /// <returns>A task that represents the application's lifetime.</returns>
    public virtual Task RunAsync(CancellationToken cancellationToken)
    {
        return Scope.BeginPhase(InitialPhaseId, cancellationToken);
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
                : new SemVersion(0, 0, 1),
        };
    }
}
