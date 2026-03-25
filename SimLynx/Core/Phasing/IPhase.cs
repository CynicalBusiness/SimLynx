using System.Threading;
using System.Threading.Tasks;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Host for a phase of execution in SimLynx.
/// </summary>
public interface IPhase
{
    /// <summary>
    /// Whether or not this phase has started execution.
    /// </summary>
    public bool HasStarted { get; }

    /// <summary>
    /// Runs this phase, returning a task that completes when the phase completes.
    /// </summary>
    /// <param name="phasePlan">An optional plan for which phases to run after this one, represented as an array of phase IDs. If the array is empty or null, no next phase will be run.</param>
    /// <param name="cancellationToken">A token to abort the run.</param>
    /// <returns>A task that represents the run operation.</returns>
    public Task StartAsync(Symbol[]? phasePlan = null, CancellationToken cancellationToken = default);
}
