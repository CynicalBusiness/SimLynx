namespace SimLynx.Core.Messaging;

/// <summary>
/// Delegate for a handler for a particular type of message.
/// </summary>
/// <typeparam name="TMessage">The type of message to handle.</typeparam>
/// <param name="letter">The message letter to handle.</param>
public delegate void MessageHandler<in TMessage>(TMessage letter)
    where TMessage : IMessage;
