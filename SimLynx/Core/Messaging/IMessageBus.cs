using System;

namespace SimLynx.Core.Messaging;

/// <summary>
/// Main bus for SimLynx's messaging system.
/// </summary>
public interface IMessageBus : IMessageDispatcher
{
    /// <summary>
    /// Subscribes to messages which are at least carrying <typeparamref name="TPayload"/> payloads.
    /// </summary>
    /// <typeparam name="TPayload">The type of message to subscribe to.</typeparam>
    /// <param name="messageHandler">The handler to invoke when a message of the relevant type is emitted.</param>
    /// <param name="priority">The priority of the subscription. Higher priority handlers will be invoked first.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
    public IDisposable Subscribe<TPayload>(MessageHandler<TPayload> messageHandler, sbyte priority);
}
