using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimLynx.Core.Hooks;

/// <summary>
/// A hook delivery strategy that delivers hook invocations to handlers in a serial manner, awaiting each handler's
/// completion before moving to the next.
/// </summary>
public class SerialHookDeliveryStrategy : IHookDeliveryStrategy
{
    /// <summary>
    /// Default instance.
    /// </summary>
    public static SerialHookDeliveryStrategy Default { get; } = new();

    /// <inheritdoc/>
    public async Task Deliver<TPayload>(
        TPayload payload,
        HookContext context,
        IReadOnlyCollection<HookHandler<TPayload>> handlers
    )
    {
        foreach (var handler in handlers)
        {
            await handler(payload, context).ConfigureAwait(false);
        }
    }
}
