using System;

namespace SimLynx.Core;

/// <summary>
/// The exception that is thrown when an operation is performed on a disposed object.
/// </summary>
public class DisposedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisposedException"/> class with a default error message.
    /// </summary>
    public DisposedException()
        : this("Object disposed") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposedException"/> class with a specified error
    /// <paramref name="message"/>.
    /// </summary>
    public DisposedException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposedException"/> class with a specified
    /// <paramref name="message"/> and a reference to the <paramref name="innerException"/> that is the cause of
    /// this exception.
    /// </summary>
    public DisposedException(string message, Exception innerException)
        : base(message, innerException) { }
}
