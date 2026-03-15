using System.Threading;
using System.Threading.Tasks;

namespace SimLynx.Simulation;

/// <summary>
/// Service responsible for running the main simulation phase of SimLynx.
/// </summary>
public interface ISimulationService
{
    /// <summary>
    /// Runs the main simulation phase of SimLynx, which is responsible for handling all simulation within SimLynx.
    /// This method will block until the simulation phase has completed.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to stop the simulation operation.</param>
    /// <returns>A task that represents the asynchronous simulation operation.</returns>
    public Task RunSimulationAsync(CancellationToken cancellationToken);
}
