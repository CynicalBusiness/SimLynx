using System;

namespace SimLynx;

/// <summary>
/// Exception that indicates a key already exists when it should not.
/// </summary>
public class DuplicateKeyException : Exception
{
    private static string GetMessage(object key, string? message = null)
    {
        var s = $"Duplicate key '{key}'";
        return message is not null ? $"{s}: {message}" : s;
    }

    /// <summary>
    /// Creates a new exception indicating the <paramref name="key"/> is a duplicate, including a
    /// <paramref name="message"/> and an inner exception.
    /// </summary>
    /// <param name="key">The duplicate key.</param>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception.</param>
    public DuplicateKeyException(object key, string message, Exception inner)
        : base(GetMessage(key, message), inner) { }

    /// <summary>
    /// Creates a new exception indicating the <paramref name="key"/> is a duplicate, including a
    /// <paramref name="message"/>.
    /// </summary>
    /// <inheritdoc cref="DuplicateKeyException(object, string, Exception)"/>
    public DuplicateKeyException(object key, string message)
        : base(GetMessage(key, message)) { }

    /// <summary>
    /// Creates a new exception indicating the <paramref name="key"/> is a duplicate.
    /// </summary>
    /// <inheritdoc cref="DuplicateKeyException(object, string, Exception)"/>
    public DuplicateKeyException(object key)
        : base(GetMessage(key)) { }
}
