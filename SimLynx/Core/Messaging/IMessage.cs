using System;

namespace SimLynx.Core.Messaging;

/// <summary>
/// A dispatched message.
/// </summary>
public interface IMessage
{
    /// <summary>
    /// The timestamp of when the message was dispatched.
    /// </summary>
    public DateTime DispatchTime { get; }

    /// <summary>
    /// The type this message was dispatched as.
    /// </summary>
    /// <remarks>
    /// This value may differ from the actual type of the message instance, as messages are not required to be
    /// dispatched <em>as</em> their actual type, but must always be assignable to the dispatch type.
    /// </remarks>
    public Type DispatchType { get; }

    /// <summary>
    /// The payload of the message.
    /// </summary>
    public object? Payload { get; }
}

/// <summary>
/// A dispatched message with a <typeparamref name="TPayload"/> payload.
/// </summary>
/// <typeparam name="TPayload">The type of the payload.</typeparam>
public interface IMessage<out TPayload> : IMessage
{
    /// <inheritdoc cref="IMessage.Payload"/>
    public new TPayload Payload { get; }
    object? IMessage.Payload => Payload;

    Type IMessage.DispatchType => typeof(TPayload);
}
