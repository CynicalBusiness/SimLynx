
using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Features.OwnedInstances;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Base implementation for a phase manager, with extra support for only-once initialization and chaining.
/// </summary>
public abstract class PhaseManager<TPhase>(
    ILifetimeScope currentScope) :
        IPhaseManager
        where TPhase : IPhase
{
    private Task? _initTask;
    private readonly Lock _initTaskLock = new();

    /// <summary>
    /// Whether or not this manager has initialized yet.
    /// </summary>
    public bool IsInitialized => _initTask is not null;

    /// <summary>
    /// The assembly load context to use for this phase, if any.
    /// </summary>
    public AssemblyLoadContext? LoadContext { get; protected set; }

    /// <summary>
    /// Resets the initialization state of this manager, allowing it to be initialized again.
    /// </summary>
    public virtual void Reset()
    {
        lock (_initTaskLock)
        {
            _initTask = null;
        }
    }

    /// <inheritdoc/>
    public async virtual Task<Owned<IPhase>> StartAsync(CancellationToken cancellationToken)
    {
        await TryInitAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        var tag = new TypedService(typeof(TPhase));
        var phaseScope = LoadContext is not null
            ? currentScope.BeginLoadContextLifetimeScope(tag, LoadContext, ConfigureContainer)
            : currentScope.BeginLifetimeScope(tag, ConfigureContainer);

        var phase = phaseScope.Resolve<TPhase>();
        cancellationToken.ThrowIfCancellationRequested();

        return new Owned<IPhase>(phase, phaseScope);
    }

    /// <summary>
    /// Performance initialization for this manager. Even if <see cref="StartAsync"/> is called more than once, this method
    /// will only be invoked once, and will be awaited during the first call to <see cref="StartAsync"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to abort the initialization process.</param>
    /// <returns>A task that represents the initialization operation.</returns>
    protected virtual Task InitAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task TryInitAsync(CancellationToken cancellationToken)
    {
        lock (_initTaskLock)
        {
            if (_initTask is null || _initTask.IsFaulted || _initTask.IsCanceled)
            {
                _initTask = InitAsync(cancellationToken);
            }
        }

        return _initTask;
    }

    /// <summary>
    /// Override this method to provide additional configuration to the phase's DI container.
    /// </summary>
    /// <param name="builder">The <see cref="ContainerBuilder"/> instance to configure.</param>
    protected virtual void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterType<TPhase>()
            .AsSelf()
            .As<IPhase>()
            .SingleInstance();
    }

}
