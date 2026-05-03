using System;

namespace SimLynx.Core.Hooks;

/// <summary>
/// Observable side of hooks, which subscribers can subscribe to in order to receive hook invocations.
/// </summary>
/// <typeparam name="TPayload">The type of the payload for the hook.</typeparam>
public interface IHookable<out TPayload>
{
    /// <summary>
    /// Subscribes a handler to the hook with the specified priority.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <returns>A disposable that can be used to unsubscribe the handler.</returns>
    public IDisposable Subscribe(IHookHandler<TPayload> handler);
}
