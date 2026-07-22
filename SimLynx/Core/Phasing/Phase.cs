using System;
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
    /// <summary>
    /// Prefix for lifetime scope tags associated with phases by phase ID.
    /// </summary>
    public const string PHASE_SCOPE_TAG_PREFIX = "Phase:";

    /// <summary>
    /// Gets the tag for lifetime scopes associated with the given <paramref name="phaseType"/>.
    /// </summary>
    /// <param name="phaseType">The type of the phase.</param>
    /// <param name="phaseId">The ID of the phase.</param>
    /// <returns>The tag for the lifetime scope associated with the phase type.</returns>
    public static PhaseScopeTag GetLifetimeScopeTag(Type phaseType, string phaseId)
    {
        ArgumentNullException.ThrowIfNull(phaseType);
        return new PhaseScopeTag(phaseType, phaseId);
    }

    /// <summary>
    /// Gets the tag for lifetime scopes associated with the given <typeparamref name="TPhase"/>.
    /// </summary>
    /// <typeparam name="TPhase">The type of the phase.</typeparam>
    /// <param name="phaseId">The ID of the phase.</param>
    /// <returns>The tag for the lifetime scope associated with the phase type.</returns>
    public static PhaseScopeTag GetLifetimeScopeTag<TPhase>(string phaseId)
        where TPhase : Phase
    {
        return GetLifetimeScopeTag(typeof(TPhase), phaseId);
    }

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
        return (Task)InvokeRunHookMethod.MakeGenericMethod(GetType()).Invoke(this, [])!;
    }

    private Task InvokeRunHook<TPhase>()
        where TPhase : Phase
    {
        var hook = Scope.Resolve<Hook<OnPhaseRun<TPhase>>>();
        return hook.Invoke(new OnPhaseRun<TPhase>((TPhase)this));
    }
}
