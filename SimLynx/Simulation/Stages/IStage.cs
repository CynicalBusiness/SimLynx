using System.Threading;
using System.Threading.Tasks;

namespace SimLynx.Simulation.Stages;

/// <summary>
/// Interface for stages within the simulation.
/// </summary>
/// <remarks>
/// Stages, as in "stages of a theater", are the top-level of the simulation, working similarly to "scenes" in other
/// game engines. Stages run completely independently and concurrently with respect to each-other, each with their
/// own set of components and systems.
/// </remarks>
public interface IStage
{
    /// <summary>
    /// The stage's name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Runs the stage. The stage will remain active until the returned task completes.
    /// </summary>
    /// <remarks>
    /// The caller can indicate the stage should stop by cancelling the provided <paramref name="cancellationToken"/>,
    /// but the stage may choose to stop for its own reasons as well.
    /// <br/>
    /// For most implementations, this method should return the same task if the method is invoked multiple times.
    /// </remarks>
    /// <param name="cancellationToken">A token to monitor for stop requests.</param>
    /// <returns>A task that represents the stage's lifetime.</returns>
    public Task RunAsync(CancellationToken cancellationToken);
}
