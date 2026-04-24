using System;
using System.Threading;

namespace SimLynx.Core.Hooks;

/// <summary>
/// Context provided to hook handlers during invocation.
/// </summary>
public class HookContext
{
    /// <summary>
    /// Timestamp indicating when the hook was dispatched.
    /// </summary>
    public required DateTime DispatchedAt { get; init; }

    /// <summary>
    /// Cancellation token that can be used by handlers to observe cancellation requests for the hook invocation.
    /// </summary>
    public required CancellationToken CancellationToken { get; init; }
}
