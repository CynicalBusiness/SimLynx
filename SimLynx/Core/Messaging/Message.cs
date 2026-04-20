using System;

namespace SimLynx.Core.Messaging;

internal record class Message<TPayload>(TPayload Payload) : IMessage<TPayload>
{
    public DateTime DispatchTime { get; init; } = DateTime.Now;

    /// <inheritdoc/>
    public override string ToString() => $"Message<{typeof(TPayload).Name}>({DispatchTime:O}, {Payload})";
}
