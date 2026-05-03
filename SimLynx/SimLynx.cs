using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Registration;

namespace SimLynx;

/// <summary>
/// Helpers for using SimLynx.
/// </summary>
public static class SimLynx
{
    /// <summary>
    /// Registers the SimLynx services with the given container builder, using a new <typeparamref name="TApp"/> instance.
    /// </summary>
    /// <typeparam name="TApp">The type of the SimLynx app.</typeparam>
    /// <param name="builder">The container builder.</param>
    /// <returns>The module registrar.</returns>
    public static IModuleRegistrar RegisterSimLynx<TApp>(this ContainerBuilder builder)
        where TApp : SimLynxApp
    {
        return builder.RegisterModule<SimLynxModule<TApp>>().IfNotRegistered(typeof(SimLynxApp));
    }

    /// <summary>
    /// Runs a new self-contained SimLynx app instance.
    /// </summary>
    /// <typeparam name="TApp">The type of the SimLynx app.</typeparam>
    /// <param name="app">The created app instance.</param>
    /// <param name="configureContainer">An optional action to configure the container before it is built.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to stop the app.</param>
    /// <returns>A task that represents the lifetime of the app.</returns>
    public static Task Run<TApp>(
        out TApp app,
        Action<ContainerBuilder>? configureContainer = null,
        CancellationToken cancellationToken = default
    )
        where TApp : SimLynxApp
    {
        var builder = new ContainerBuilder();
        builder.RegisterSimLynx<TApp>();
        configureContainer?.Invoke(builder);
        using var container = builder.Build();

        app = container.Resolve<TApp>();
        return app.RunAsync(cancellationToken);
    }

    /// <inheritdoc cref="Run{TApp}(out TApp, Action{ContainerBuilder}?, CancellationToken)"/>
    public static Task Run<TApp>(out TApp app, CancellationToken cancellationToken = default)
        where TApp : SimLynxApp
    {
        return Run(out app, null, cancellationToken);
    }
}
