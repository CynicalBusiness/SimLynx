using Microsoft.Extensions.Logging;

namespace SimLynx.Core.Logging;

/// <summary>
/// Represents a log event in SimLynx, encapsulating an <see cref="Microsoft.Extensions.Logging.EventId"/>, a message template, and a log level.
/// </summary>
/// <remarks>
/// Functions as a helper wrapper around <see cref="LogActions.Define(EventId, string, LogLevel)"/> (and by extension
/// <see cref="LoggerMessage.Define(LogLevel, EventId, string)"/>) to allow for more concise log event definitions and
/// deduplicated usage.
/// </remarks>
/// <param name="EventId">The unique identifier for the log event.</param>
/// <param name="MessageTemplate">The message template for the log event.</param>
/// <param name="LogLevel">The severity level of the log event.</param>
public record LogEvent(EventId EventId, string MessageTemplate, LogLevel LogLevel = LogLevel.Information)
{
    /// <summary>
    /// Logs this event.
    /// </summary>
    public LogAction Log = LogActions.Define(EventId, MessageTemplate, LogLevel);
}

/// <inheritdoc cref="LogEvent"/>
public record LogEvent<T1>(EventId EventId, string MessageTemplate, LogLevel LogLevel = LogLevel.Information)
{
    /// <summary>
    /// Logs this event with the given argument.
    /// </summary>
    public LogAction<T1> Log = LogActions.Define<T1>(EventId, MessageTemplate, LogLevel);
}

/// <inheritdoc cref="LogEvent"/>
public record LogEvent<T1, T2>(EventId EventId, string MessageTemplate, LogLevel LogLevel = LogLevel.Information)
{
    /// <summary>
    /// Logs this event with the given arguments.
    /// </summary>
    public LogAction<T1, T2> Log = LogActions.Define<T1, T2>(EventId, MessageTemplate, LogLevel);
}

/// <inheritdoc cref="LogEvent"/>
public record LogEvent<T1, T2, T3>(EventId EventId, string MessageTemplate, LogLevel LogLevel = LogLevel.Information)
{
    /// <summary>
    /// Logs this event with the given arguments.
    /// </summary>
    public LogAction<T1, T2, T3> Log = LogActions.Define<T1, T2, T3>(EventId, MessageTemplate, LogLevel);
}

/// <inheritdoc cref="LogEvent"/>
public record LogEvent<T1, T2, T3, T4>(
    EventId EventId,
    string MessageTemplate,
    LogLevel LogLevel = LogLevel.Information
)
{
    /// <summary>
    /// Logs this event with the given arguments.
    /// </summary>
    public LogAction<T1, T2, T3, T4> Log = LogActions.Define<T1, T2, T3, T4>(EventId, MessageTemplate, LogLevel);
}

/// <inheritdoc cref="LogEvent"/>
public record LogEvent<T1, T2, T3, T4, T5>(
    EventId EventId,
    string MessageTemplate,
    LogLevel LogLevel = LogLevel.Information
)
{
    /// <summary>
    /// Logs this event with the given arguments.
    /// </summary>
    public LogAction<T1, T2, T3, T4, T5> Log = LogActions.Define<T1, T2, T3, T4, T5>(
        EventId,
        MessageTemplate,
        LogLevel
    );
}
