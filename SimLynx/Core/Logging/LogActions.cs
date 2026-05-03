using Microsoft.Extensions.Logging;

namespace SimLynx.Core.Logging;

/// <summary>
/// Helper class for defining log messages, adapting off of <see cref="LoggerMessage.Define(LogLevel, EventId, string)"/>.
/// </summary>
public static class LogActions
{
    /// <summary>
    /// Defines a log message with the given event name, message template, and log level, associated with the specified scope type.
    /// </summary>
    /// <param name="eventId">The event ID for the log message.</param>
    /// <param name="message">The message template for the log message.</param>
    /// <param name="logLevel">The log level for the log message.</param>
    /// <returns>A <see cref="LogAction"/> that can be used to log the defined message.</returns>
    public static LogAction Define(EventId eventId, string message, LogLevel logLevel = LogLevel.Information)
    {
        var action = LoggerMessage.Define(logLevel, eventId, message);
        return (logger, exception) =>
        {
            if (logger is not null)
            {
                action.Invoke(logger, exception);
            }
        };
    }

    /// <inheritdoc cref="Define(EventId, string, LogLevel)"/>
    public static LogAction<T1> Define<T1>(EventId eventId, string message, LogLevel logLevel = LogLevel.Information)
    {
        var action = LoggerMessage.Define<T1>(logLevel, eventId, message);
        return (logger, arg1, exception) =>
        {
            if (logger is not null)
            {
                action.Invoke(logger, arg1, exception);
            }
        };
    }

    /// <inheritdoc cref="Define(EventId, string, LogLevel)"/>
    public static LogAction<T1, T2> Define<T1, T2>(
        EventId eventId,
        string message,
        LogLevel logLevel = LogLevel.Information
    )
    {
        var action = LoggerMessage.Define<T1, T2>(logLevel, eventId, message);
        return (logger, arg1, arg2, exception) =>
        {
            if (logger is not null)
            {
                action.Invoke(logger, arg1, arg2, exception);
            }
        };
    }

    /// <inheritdoc cref="Define(EventId, string, LogLevel)"/>
    public static LogAction<T1, T2, T3> Define<T1, T2, T3>(
        EventId eventId,
        string message,
        LogLevel logLevel = LogLevel.Information
    )
    {
        var action = LoggerMessage.Define<T1, T2, T3>(logLevel, eventId, message);
        return (logger, arg1, arg2, arg3, exception) =>
        {
            if (logger is not null)
            {
                action.Invoke(logger, arg1, arg2, arg3, exception);
            }
        };
    }

    /// <inheritdoc cref="Define(EventId, string, LogLevel)"/>
    public static LogAction<T1, T2, T3, T4> Define<T1, T2, T3, T4>(
        EventId eventId,
        string message,
        LogLevel logLevel = LogLevel.Information
    )
    {
        var action = LoggerMessage.Define<T1, T2, T3, T4>(logLevel, eventId, message);
        return (logger, arg1, arg2, arg3, arg4, exception) =>
        {
            if (logger is not null)
            {
                action.Invoke(logger, arg1, arg2, arg3, arg4, exception);
            }
        };
    }

    /// <inheritdoc cref="Define(EventId, string, LogLevel)"/>
    public static LogAction<T1, T2, T3, T4, T5> Define<T1, T2, T3, T4, T5>(
        EventId eventId,
        string message,
        LogLevel logLevel = LogLevel.Information
    )
    {
        var action = LoggerMessage.Define<T1, T2, T3, T4, T5>(logLevel, eventId, message);
        return (logger, arg1, arg2, arg3, arg4, arg5, exception) =>
        {
            if (logger is not null)
            {
                action.Invoke(logger, arg1, arg2, arg3, arg4, arg5, exception);
            }
        };
    }
}
