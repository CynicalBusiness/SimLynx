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
public abstract class SimLynxApp : IContentPackage
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
    /// The Autofac scope for this application.
    /// </summary>
    protected ILifetimeScope Scope { get; }

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
        return Scope.BeginPhase(CreatePhasePlan(), cancellationToken);
    }

    /// <summary>
    /// Creates a new phase plan for the application.
    /// </summary>
    /// <returns>The created phase plan.</returns>
    public PhasePlan CreatePhasePlan()
    {
        var builder = new PhasePlan.Builder();
        ConfigurePhasePlan(builder);
        return builder.Build();
    }

    /// <summary>
    /// Override to configure the phase plan for the main app. The default implementation configures the standard
    /// SimLynx phases in the order of Discovery, Design, and Simulation.
    /// </summary>
    /// <param name="builder">The phase plan builder to configure.</param>
    protected virtual void ConfigurePhasePlan(PhasePlan.Builder builder)
    {
        builder.Then<DiscoveryPhase>().Then<DesignPhase>().Then<SimulationPhase>();
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
            Id = Symbol.For(thisType.Namespace ?? thisType.Name),
            Version = thisAssemblyVersion is not null
                ? SemVersion.FromVersion(thisAssemblyVersion)
                : new SemVersion(0, 0, 1),
        };
    }
}
