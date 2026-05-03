using System.Threading;
using System.Threading.Tasks;

namespace SimLynx.Core.Hooks;

/// <summary>
/// Interface representing a type that can be invoked like a hook.
/// </summary>
/// <typeparam name="TPayload">The payload type for the hook.</typeparam>
public interface IHookInvocable<in TPayload>
{
    /// <summary>
    /// Invokes the hook with the given <paramref name="payload"/>, observing the provided
    /// <paramref name="cancellationToken"/> for cancellation requests.
    /// </summary>
    /// <param name="payload">The payload to pass to the hook.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task Invoke(TPayload payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invokes the hook with the given <paramref name="payload"/> with the provided <paramref name="context"/>.
    /// </summary>
    /// <param name="payload">The payload to pass to the hook.</param>
    /// <param name="context">The context to pass to the hook.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task Invoke(TPayload payload, HookContext context);
}
