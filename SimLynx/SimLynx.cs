
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace SimLynx;

/// <summary>
/// Helpers for using SimLynx.
/// </summary>
public static class SimLynx
{

    /// <summary>
    /// Runs the given SimLynx application in self-contained mode, starting the simulation and returning a task that
    /// completes when the simulation stops.
    /// </summary>
    /// <remarks>
    /// This method is the most common approach to starting SimLynx and should be used when no other dependency
    /// injection container is being used.
    /// </remarks>
    /// <param name="app">The SimLynx application to run.</param>
    /// <param name="cancellationToken">The cancellation token to stop the simulation.</param>
    /// <returns>A task that completes when the simulation stops.</returns>
    public static async Task RunAsync<TApp>(TApp app, CancellationToken cancellationToken = default)
        where TApp : SimLynxApplication
    {
        var services = new ServiceCollection();
        services.AddLogging();
        app.AddServices(services);

        var builder = new ContainerBuilder();
        builder.Populate(services);
        builder.RegisterModule(app);

        await using var container = builder.Build();
        var host = container.Resolve<SimLynxHost>();
        app.ConfigureHost(host);

        await host.StartAsync(cancellationToken);
    }

    /// <inheritdoc cref="RunAsync{TApp}(TApp, CancellationToken)"/>
    public static Task RunAsync<TApp>(out TApp app, CancellationToken cancellationToken = default)
        where TApp : SimLynxApplication, new()
    {
        app = new TApp();
        return RunAsync(app, cancellationToken);
    }

    /// <inheritdoc cref="RunAsync{TApp}(TApp, CancellationToken)"/>
    public static Task RunAsync<TApp>(CancellationToken cancellationToken = default)
        where TApp : SimLynxApplication, new()
    {
        return RunAsync<TApp>(out _, cancellationToken);
    }

}
