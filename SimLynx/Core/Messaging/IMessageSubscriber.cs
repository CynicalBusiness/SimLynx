using System;
using System.Collections.Generic;

namespace SimLynx.Core.Messaging;

/// <summary>
/// Subscriber to messages sent through SimLynx's messaging system.
/// </summary>
public interface IMessageSubscriber
{
    /// <summary>
    /// Priority of the subscriber, where higher values will be executed before lower values, with ties being broken
    /// by registration order.
    /// </summary>
    public sbyte Priority { get; }

    /// <summary>
    /// Handles a message.
    /// </summary>
    /// <param name="message">The message instance to handle.</param>
    public void HandleMessage(IMessage message);
}

/// <summary>
/// Subscriber to <typeparamref name="TMessage"/> messages sent through SimLynx's messaging system.
/// </summary>
/// <typeparam name="TMessage">The type of message to subscribe to.</typeparam>
public interface IMessageSubscriber<in TMessage> : IMessageSubscriber
{
    /// <summary>
    /// Handles a message of at least the specified type.
    /// </summary>
    /// <param name="message">The message instance to handle.</param>
    public void HandleMessage(TMessage message);

    void IMessageSubscriber.HandleMessage(IMessage message)
    {
        if (message is TMessage typedMessage)
        {
            HandleMessage(typedMessage);
        }
    }
}
