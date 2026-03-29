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

/// <summary>
/// Management handle for a running SimLynx instance.
/// </summary>
/// <typeparam name="TApp"></typeparam>
public class SimLynx<TApp>
    where TApp : SimLynxApp
{
    private readonly ReaderWriterLockSlim _lock = new();
    private Handle? handle;

    internal SimLynx(TApp app)
    {
        App = app;
    }

    /// <summary>
    /// The app used for this SimLynx instance.
    /// </summary>
    public TApp App { get; }

    /// <summary>
    /// The cancellation token that can be used to stop the SimLynx instance.
    /// </summary>
    public CancellationToken StopToken { get; }

    /// <summary>
    /// Whether the SimLynx instance is currently running.
    /// </summary>
    public bool IsRunning
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return handle is not null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        _lock.EnterUpgradeableReadLock();
        try
        {
            if (handle is not null)
            {
                throw new InvalidOperationException("SimLynx instance is already running");
            }

            _lock.EnterWriteLock();
            try
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                handle = new Handle(cts, App.RunAsync(cts.Token));
                return handle.Task;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Stops the SimLynx instance.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public bool Stop()
    {
        _lock.EnterUpgradeableReadLock();
        try
        {
            if (handle is null)
            {
                return false;
            }

            _lock.EnterWriteLock();
            try
            {
                handle.StopTokenSource.Cancel();
                handle = null;
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    private record Handle(CancellationTokenSource StopTokenSource, Task Task);
}
