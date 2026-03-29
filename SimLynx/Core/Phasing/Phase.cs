using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Base implementation for a phase of execution in SimLynx, with support for chaining to a next phase.
/// </summary>
public abstract class Phase(IIndex<Symbol, IPhaseManager> phaseManagers) : IPhase
{
    private int _startFlag = 0;

    /// <inheritdoc/>
    public bool HasStarted => _startFlag != 0;

    /// <inheritdoc/>
    public async Task StartAsync(Symbol[]? phasePlan = null, CancellationToken cancellationToken = default)
    {
        var currentStatus = Interlocked.CompareExchange(ref _startFlag, 1, 0);
        if (currentStatus != 0)
        {
            throw new InvalidOperationException($"Cannot start already-started phase '{GetType().Name}'");
        }

        cancellationToken.ThrowIfCancellationRequested();

        await RunAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        Symbol? nextPhaseId = null;
        Symbol[]? nextPhasePlan = null;
        if (phasePlan is not null && phasePlan.Length > 0)
        {
            nextPhaseId = phasePlan[0];
            nextPhasePlan = phasePlan.Length > 1 ? phasePlan[1..] : null;
        }

        if (nextPhaseId is not null)
        {
            var nextManager = phaseManagers[nextPhaseId.Value];
            await nextManager.StartAndRunAsync(nextPhasePlan, cancellationToken);
        }
    }

    /// <summary>
    /// Executes the logic of this phase, returning a task that completes when the phase is complete.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    protected abstract Task RunAsync(CancellationToken cancellationToken);
}
