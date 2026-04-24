using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Manager responsible for orchestrating the execution of phases in SimLynx.
/// </summary>
public interface IPhaseManager
{
    /// <summary>
    /// Starts the execution of the phase with the given ID, then executes the next phase once it completes, and so
    /// on, returning a task that completes once a phase is reached that does not specify a next phase and it completes.
    /// </summary>
    /// <param name="phaseId">The ID of the phase to start.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the run of each phase in the chain.</returns>
    /// <exception cref="ArgumentException">If no phase builder is registered for the given phase ID.</exception>
    public Task StartAsync(string phaseId, CancellationToken cancellationToken = default);
}
