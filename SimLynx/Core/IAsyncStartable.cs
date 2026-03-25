using System.Threading;
using System.Threading.Tasks;

namespace SimLynx.Core;

/// <summary>
/// Interface for components that can be asynchronously started, such as during the application startup process.
/// </summary>
/// <remarks>
/// Similar to <see cref="Autofac.IStartable"/>, but async.
/// </remarks>
public interface IAsyncStartable
{
    /// <summary>
    /// Asynchronously starts the component.
    /// </summary>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    public Task StartAsync(CancellationToken cancellationToken = default);
}
