using System.Threading;
using System.Threading.Tasks;
using Autofac;

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
    /// The DI scope associated with this phase.
    /// </summary>
    public ILifetimeScope Scope { get; }

    /// <summary>
    /// The ID of the next phase to run after this one, if any.
    /// </summary>
    public string? NextPhaseId { get; set; }

    /// <summary>
    /// Runs this phase, returning a task that completes when the phase completes.
    /// </summary>
    /// <param name="cancellationToken">A token to abort the run.</param>
    /// <returns>A task that represents the run operation.</returns>
    public Task RunAsync(CancellationToken cancellationToken = default);
}
