using System;
using System.Threading.Tasks;

namespace SimLynx.Core.Messaging;

/// <summary>
/// Extensions related to the messaging system.
/// </summary>
public static class MessagingExtensions
{
    extension(IMessageBus @this)
    {
        /// <summary>
        /// Subscribes a handler to this bus with the default priority.
        /// </summary>
        /// <typeparam name="TPayload">The type of the message payload.</typeparam>
        /// <param name="messageHandler">The handler to subscribe.</param>
        /// <returns>A disposable that can be used to unsubscribe the handler.</returns>
        public IDisposable Subscribe<TPayload>(MessageHandler<TPayload> messageHandler)
            where TPayload : notnull
        {
            return @this.Subscribe(messageHandler, Priorities.Normal);
        }

        /// <summary>
        /// Subscribes a synchronous action as a handler to this bus with the given <paramref name="priority"/>.
        /// </summary>
        /// <typeparam name="TPayload">The type of the message payload.</typeparam>
        /// <param name="messageAction">The handler to subscribe.</param>
        /// <param name="priority">The priority of the handler.</param>
        /// <returns>A disposable that can be used to unsubscribe the handler.</returns>
        public IDisposable Subscribe<TPayload>(Action<IMessage<TPayload>> messageAction, sbyte priority)
            where TPayload : notnull
        {
            return @this.Subscribe<TPayload>(
                (message, _) =>
                {
                    messageAction.Invoke(message);
                    return Task.CompletedTask;
                },
                priority
            );
        }

        /// <summary>
        /// Subscribes a synchronous action as a handler to this bus with the default priority.
        /// </summary>
        /// <typeparam name="TPayload">The type of the message payload.</typeparam>
        /// <param name="messageAction">The handler to subscribe.</param>
        /// <returns>A disposable that can be used to unsubscribe the handler.</returns>
        public IDisposable Subscribe<TPayload>(Action<IMessage<TPayload>> messageAction)
            where TPayload : notnull
        {
            return @this.Subscribe(messageAction, Priorities.Normal);
        }

        /// <summary>
        /// Subscribes the provided <paramref name="subscriber"/> to this bus with the subscriber's defined priority.
        /// </summary>
        /// <typeparam name="TPayload">The type of the message payload.</typeparam>
        /// <param name="subscriber">The subscriber to add.</param>
        /// <returns>A disposable that can be used to unsubscribe the subscriber.</returns>
        public IDisposable Subscribe<TPayload>(IMessageSubscriber<TPayload> subscriber)
        {
            return @this.Subscribe<TPayload>(subscriber.HandleMessage, subscriber.Priority);
        }
    }
}
