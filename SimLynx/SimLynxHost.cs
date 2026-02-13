
using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Features.OwnedInstances;
using Microsoft.Extensions.Logging;
using SimLynx.Core;
using SimLynx.Design;

namespace SimLynx;

/// <summary>
/// Host for SimLynx.
/// </summary>
public class SimLynxHost(
    ILifetimeScope scope,
    ILogger<SimLynxHost> logger,
    RuntimeRegister runtimeRegister,
    DesignRegister designRegister)
{
    /// <summary>
    /// Whether or not the design phase has been run.
    /// </summary>
    public bool HasDesigned { get; private set; } = false;

    /// <summary>
    /// Register for the runtime scope of SimLynx.
    /// </summary>
    public RuntimeRegister Runtime { get; } = runtimeRegister;

    /// <summary>
    /// Register for the design-time scope of SimLynx.
    /// </summary>
    public DesignRegister Design { get; } = designRegister;

    /// <summary>
    /// Begins the design-time of SimLynx.
    /// </summary>
    /// <returns>An owned instance of the design host, which will dispose the design scope when disposed.</returns>
    protected virtual Owned<DesignHost> StartDesign()
    {
        var tag = new TypedService(typeof(DesignHost));
        var designScope = scope.BeginLifetimeScope(
            tag,
            builder =>
            {
                builder.RegisterModule(Design);
            });

        var host = designScope.Resolve<DesignHost>();
        return new Owned<DesignHost>(host, designScope);
    }

    /// <summary>
    /// Begins the runtime of SimLynx.
    /// </summary>
    /// <returns>An owned instance of the runtime host, which will dispose the runtime scope when disposed.</returns>
    protected virtual Owned<RuntimeHost> StartRuntime()
    {
        var tag = new TypedService(typeof(RuntimeHost));
        var runtimeScope = scope.BeginLifetimeScope(
            tag,
            builder =>
            {
                builder.RegisterModule(Runtime);
            });

        var host = runtimeScope.Resolve<RuntimeHost>();
        return new Owned<RuntimeHost>(host, runtimeScope);
    }

    /// <summary>
    /// Starts the SimLynx simulation, running the design phase if not already.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task which completes after runtime has stopped.</returns>
    public virtual async Task StartAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!HasDesigned)
        {
            logger.LogInformation("Staring design phase...");
            await using var designHost = StartDesign();
            HasDesigned = true;
        }
        else
        {
            logger.LogDebug("Design phase has already been run, skipping...");
        }

        await using var runtimeHost = StartRuntime();
        logger.LogInformation("Starting runtime phase...");

        cancellationToken.Register(() =>
        {
            runtimeHost.Value.Stop();
        });

        await Task.FromCanceled(runtimeHost.Value.StopSource.Token);
    }
}
