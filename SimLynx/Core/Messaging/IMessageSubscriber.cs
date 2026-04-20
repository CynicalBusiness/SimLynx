using System.Threading;
using System.Threading.Tasks;

namespace SimLynx.Core.Messaging;

/// <summary>
/// Marker interface for message subscribers.
/// </summary>
public interface IMessageSubscriber
{
    // intentionally blank
}

/// <summary>
/// Interface for subscribers to messages with a <typeparamref name="TPayload"/> payload.
/// </summary>
/// <typeparam name="TPayload">The type of the message payload.</typeparam>
public interface IMessageSubscriber<in TPayload> : IMessageSubscriber
{
    /// <summary>
    /// The priority of this subscriber.
    /// </summary>
    public sbyte Priority { get; }

    /// <summary>
    /// Handles a message with a <typeparamref name="TPayload"/> payload.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task HandleMessage(IMessage<TPayload> message, CancellationToken cancellationToken);
}
