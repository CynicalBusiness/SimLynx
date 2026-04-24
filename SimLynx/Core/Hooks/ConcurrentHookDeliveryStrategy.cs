using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimLynx.Core.Hooks;

/// <summary>
/// Hook delivery strategy that delivers to all handlers concurrently and waits for all to complete before returning.
/// </summary>
/// <remarks>
/// This strategy uses <see cref="Task.WhenAll(Task[])"/> to execute all handlers concurrently.
/// </remarks>
public class ConcurrentHookDeliveryStrategy : IHookDeliveryStrategy
{
    /// <summary>
    /// Default instance.
    /// </summary>
    public static ConcurrentHookDeliveryStrategy Default { get; } = new();

    /// <inheritdoc/>
    public Task Deliver<TPayload>(
        TPayload payload,
        HookContext context,
        IReadOnlyCollection<HookHandler<TPayload>> handlers
    )
    {
        return Task.WhenAll(handlers.Select(handler => handler.Invoke(payload, context)));
    }
}
