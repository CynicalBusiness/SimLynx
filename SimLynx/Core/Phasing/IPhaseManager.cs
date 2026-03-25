using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.OwnedInstances;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Generalized manager for a phase of execution in SimLynx.
/// </summary>
public interface IPhaseManager
{
    /// <summary>
    /// Starts this manager's phase, returning a task that completes once the phase is running.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to abort the startup process.</param>
    public Task<Owned<IPhase>> StartAsync(CancellationToken cancellationToken);
}
