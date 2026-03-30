using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Features.OwnedInstances;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Base implementation for a phase manager, with extra support for only-once initialization and chaining.
/// </summary>
public abstract class PhaseBuilder<TPhase>(ILifetimeScope currentScope) : IPhaseBuilder<TPhase>
    where TPhase : IPhase
{
    private Task? _initTask;
    private readonly ReaderWriterLockSlim _initTaskLock = new();

    /// <summary>
    /// Whether or not this manager has initialized yet.
    /// </summary>
    public bool IsInitialized => _initTask is not null;

    /// <summary>
    /// Resets the initialization state of this manager, allowing it to be initialized again.
    /// </summary>
    public virtual void Reset()
    {
        _initTaskLock.EnterWriteLock();
        try
        {
            _initTask = null;
        }
        finally
        {
            _initTaskLock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public virtual async Task<TPhase> BuildAsync(CancellationToken cancellationToken)
    {
        await TryInit(cancellationToken);

        var phaseScope = currentScope.BeginLifetimeScope(new TypedService(typeof(TPhase)), ConfigureContainer);
        var phase = phaseScope.Resolve<TPhase>();
        return phase;
    }

    /// <summary>
    /// Performance initialization for this manager. Even if <see cref="BuildAsync"/> is called more than once, this method
    /// will only be invoked once, and will be awaited during the first call to <see cref="BuildAsync"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to abort the initialization process.</param>
    /// <returns>A task that represents the initialization operation.</returns>
    protected virtual Task Init(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Override this method to provide additional configuration to the phase's DI container.
    /// </summary>
    /// <param name="builder">The <see cref="ContainerBuilder"/> instance to configure.</param>
    protected virtual void ConfigureContainer(ContainerBuilder builder)
    {
        // do nothing by default
    }

    private Task TryInit(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _initTaskLock.EnterUpgradeableReadLock();
        try
        {
            if (_initTask is not null)
            {
                return _initTask;
            }

            _initTaskLock.EnterWriteLock();
            try
            {
                return _initTask = Init(cancellationToken);
            }
            finally
            {
                _initTaskLock.ExitWriteLock();
            }
        }
        finally
        {
            _initTaskLock.ExitUpgradeableReadLock();
        }
    }
}
