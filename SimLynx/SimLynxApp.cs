using System.Threading;
using System.Threading.Tasks;
using Autofac;
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
/// <param name="scope">The Autofac scope for the application.</param>
/// <param name="manifest">The content package manifest for the main application.</param>
public abstract class SimLynxApp(ILifetimeScope scope, ContentPackageManifest manifest) : ContentPackage(manifest)
{
    /// <summary>
    /// The Autofac scope for this application.
    /// </summary>
    protected ILifetimeScope Scope { get; } = scope;

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
        ContentAttributionRegistry.Current = new(this);
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
}
