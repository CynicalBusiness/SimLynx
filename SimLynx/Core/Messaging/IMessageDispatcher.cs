using System.Threading;
using System.Threading.Tasks;

namespace SimLynx.Core.Messaging;

/// <summary>
/// Dispatcher for messages.
/// </summary>
public interface IMessageDispatcher
{
    /// <summary>
    /// Sends a <typeparamref name="TPayload"/> message to this bus.
    /// </summary>
    /// <typeparam name="TPayload">The type of message to send.</typeparam>
    /// <param name="payload">The payload of the message to send.</param>
    /// <param name="cancellationToken">A token to abort event dispatching.</param>
    /// <returns>A task that completes with the sent message once it has been processed by all relevant handlers.</returns>
    public Task<IMessage<TPayload>> Send<TPayload>(TPayload payload, CancellationToken cancellationToken = default);
}
