
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace SimLynx.Design;

/// <summary>
/// Interface for services that are used during the design phase, used to configure the simulation phase's container.
/// </summary>
public interface IDesignService
{

    /// <summary>
    /// Performs this service's design operations.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    public Task RunDesignAsync(CancellationToken cancellationToken);

}
