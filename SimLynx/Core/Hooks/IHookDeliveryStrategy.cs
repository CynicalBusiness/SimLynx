using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimLynx.Core.Hooks;

/// <summary>
/// Defines a strategy for delivering hook invocations to subscribers. This allows for different delivery mechanisms,
/// such as sync/async, serial/parallel, or other more complex strategies.
/// </summary>
public interface IHookDeliveryStrategy
{
    /// <summary>
    /// Delivers a hook invocation to the provided handlers according to the strategy's rules.
    /// </summary>
    /// <typeparam name="TPayload">The type of payload the hook carries.</typeparam>
    /// <param name="payload">The payload to deliver to the handlers.</param>
    /// <param name="context">The context of the hook invocation.</param>
    /// <param name="handlers">The handlers to which the payload should be delivered, keyed and sorted by priority.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task Deliver<TPayload>(
        TPayload payload,
        HookContext context,
        IReadOnlyDictionary<sbyte, IReadOnlyCollection<IHookHandler<TPayload>>> handlers
    );
}
