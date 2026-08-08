using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimLynx.Core.Hooks;

/// <summary>
/// A hook delivery strategy that delivers hook invocations to handlers in a serial manner, awaiting each handler's
/// completion before moving to the next.
/// </summary>
/// <remarks>
/// This strategy is used by default for most hooks unless configured otherwise.
/// </remarks>
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
        IReadOnlyDictionary<sbyte, IReadOnlyCollection<IHookHandler<TPayload>>> handlers
    )
    {
        foreach (var handlerGroup in handlers)
        {
            foreach (var handler in handlerGroup.Value)
            {
                await handler.HandleHook(payload, context).ConfigureAwait(false);
            }
        }
    }
}
