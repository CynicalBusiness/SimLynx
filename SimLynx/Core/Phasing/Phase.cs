using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Base implementation for a phase of execution in SimLynx, with support for chaining to a next phase.
/// </summary>
public abstract class Phase : IPhase
{
    private Task? runTask;

    /// <inheritdoc/>
    public bool HasStarted => runTask is not null;

    /// <inheritdoc/>
    public Symbol? NextPhaseId { get; set; }

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
    protected abstract Task RunPhase(CancellationToken cancellationToken);
}
