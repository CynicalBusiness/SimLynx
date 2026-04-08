using System;

namespace SimLynx.Core.Messaging;

/// <summary>
/// Main bus for SimLynx's messaging system.
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Subscribes to messages which are at least a(n) <typeparamref name="TMessage"/>.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to subscribe to.</typeparam>
    /// <param name="subscriber">The subscriber instance.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
    public IDisposable Subscribe<TMessage>(IMessageSubscriber<TMessage> subscriber)
        where TMessage : IMessage;

    /// <summary>
    /// Emits a <typeparamref name="TMessage"/> message.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to emit.</typeparam>
    /// <param name="message">The message instance to emit.</param>
    /// <returns>The current <see cref="IMessageBus"/> instance for chaining.</returns>
    public IMessageBus Emit<TMessage>(in TMessage message)
        where TMessage : IMessage;
}
