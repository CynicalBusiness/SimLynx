
using System.Threading;

namespace SimLynx.Core;

/// <summary>
/// The runtime host is responsible for handling all runtime operations within the SimLynx simulation.
/// </summary>
public class RuntimeHost
{

    /// <summary>
    /// A cancellation token source that can be used to signal the simulation to stop.
    /// </summary>
    public CancellationTokenSource StopSource { get; } = new();

    /// <summary>
    /// Triggers the simulation to stop by canceling the <see cref="StopSource"/>.
    /// </summary>
    /// <remarks>
    /// Does nothing else by itself and will only signal stop if not already signaled, relying on the main application
    /// to handle the request and stop the simulation.
    /// </remarks>
    public void Stop()
    {
        StopSource.Cancel();
    }

}
