using System;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Exception indicating that a prototype could not be compiled due to an error in its configuration or structure.
/// </summary>
public class PrototypeCompilationException : Exception
{
    /// <summary>
    /// Creates a new <see cref="PrototypeCompilationException"/> with a specified error message and a reference to
    /// an inner exception that caused this exception.
    /// </summary>
    /// <param name="prototype">The prototype that caused the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public PrototypeCompilationException(IPrototype prototype, string message, Exception innerException)
        : base(message, innerException)
    {
        Prototype = prototype;
    }

    /// <summary>
    /// Creates a new <see cref="PrototypeCompilationException"/> with a specified error message.
    /// </summary>
    /// <param name="prototype">The prototype that caused the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public PrototypeCompilationException(IPrototype prototype, string message)
        : base(message)
    {
        Prototype = prototype;
    }

    /// <summary>
    /// The prototype that caused the exception.
    /// </summary>
    public IPrototype Prototype { get; }
}
