using System.Threading;
using System.Threading.Tasks;

namespace SimLynx.Core.Messaging;

/// <summary>
/// Delegate for handling of <typeparamref name="TMessage"/> messages.
/// </summary>
/// <typeparam name="TMessage">The type of the message.</typeparam>
/// <param name="message">The message to handle.</param>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <returns>A task that represents the operation.</returns>
public delegate Task MessageHandler<in TMessage>(IMessage<TMessage> message, CancellationToken cancellationToken);
