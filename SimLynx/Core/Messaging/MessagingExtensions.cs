using System;
using System.Collections;
using System.Collections.Generic;
using Autofac.Builder;

namespace SimLynx.Core.Messaging;

/// <summary>
/// Extensions related to the messaging system.
/// </summary>
public static class MessagingExtensions
{
    extension(IMessageBus @this)
    {
        /// <inheritdoc cref="IMessageBus.Subscribe{TMessage}(IMessageSubscriber{TMessage})"/>
        /// <param name="handler">The delegate handler for the message.</param>
        /// <param name="priority">The priority of the subscriber.</param>
        public IDisposable Subscribe<TMessage>(
            MessageHandler<TMessage> handler,
            sbyte priority = MessageSubscriberPriority.NORMAL
        )
            where TMessage : IMessage
        {
            return @this.Subscribe(new MessageDelegateSubscriber<TMessage>(handler, priority));
        }

        /// <summary>
        /// Emits multiple <typeparamref name="TMessage"/> messages.
        /// </summary>
        /// <typeparam name="TMessage">The type of messages to emit.</typeparam>
        /// <param name="messages">The messages to emit.</param>
        /// <returns>The current <see cref="IMessageBus"/> instance for chaining.</returns>
        public IMessageBus Emit<TMessage>(IEnumerable<TMessage> messages)
            where TMessage : IMessage
        {
            foreach (var message in messages)
            {
                @this = @this.Emit(message);
            }
            return @this;
        }

        /// <inheritdoc cref="Emit{TMessage}(IMessageBus,IEnumerable{TMessage})"/>
        public IMessageBus Emit<TMessage>(params TMessage[] messages)
            where TMessage : IMessage
        {
            return @this.Emit((IEnumerable<TMessage>)messages);
        }
    }

    private record MessageDelegateSubscriber<TMessage>(MessageHandler<TMessage> Handler, sbyte Priority)
        : IMessageSubscriber<TMessage>
        where TMessage : IMessage
    {
        public void HandleMessage(TMessage message) => Handler(message);
    }
}
