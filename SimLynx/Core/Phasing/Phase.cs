using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using SimLynx.Core.Hooks;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Base implementation for a phase of execution in SimLynx, with support for chaining to a next phase.
/// </summary>
public abstract class Phase : IPhase
{
    private static readonly MethodInfo InvokeRunHookMethod = typeof(Phase).GetMethod(
        nameof(InvokeRunHook),
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;

    private Task? runTask;

    /// <inheritdoc/>
    public bool HasStarted => runTask is not null;

    /// <inheritdoc/>
    public string? NextPhaseId { get; set; }

    /// <inheritdoc/>
    public required ILifetimeScope Scope { get; init; }

    /// <inheritdoc/>
    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        return runTask ??= RunPhase(cancellationToken);
    }

    /// <summary>
    /// Executes the logic of this phase, returning a task that completes when the phase is complete.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    protected virtual Task RunPhase(CancellationToken cancellationToken)
    {
        return (Task)InvokeRunHookMethod.MakeGenericMethod(GetType()).Invoke(this, []);
    }

    private Task InvokeRunHook<TPhase>()
        where TPhase : Phase
    {
        var hook = Scope.Resolve<Hook<OnPhaseRun<TPhase>>>();
        return hook.Invoke(new OnPhaseRun<TPhase>((TPhase)this));
    }
}
