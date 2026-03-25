using System.Threading;
using System.Threading.Tasks;

namespace SimLynx.Discovery;

/// <summary>
/// Interface for services that are used during the discovery phase, used to configure the design phase's container.
/// </summary>
public interface IDiscoveryService
{
    /// <summary>
    /// Performs this service's discovery operations.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to stop the discovery operation.</param>
    /// <returns>A task that represents the asynchronous discovery operation.</returns>
    public Task RunDiscoveryAsync(CancellationToken cancellationToken);
}
