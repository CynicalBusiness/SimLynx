using System;
using Microsoft.Extensions.Logging;

namespace SimLynx.Core.Logging;

/// <summary>
/// Options for configuring logging in SimLynx.
/// </summary>
public record LoggingOptions
{
    /// <summary>
    /// The default logging options.
    /// </summary>
    public static readonly LoggingOptions Default = new();

    /// <summary>
    /// Controls what logging configuration SimLynx is responsible for.
    /// </summary>
    public LoggerSetupFlags Setup { get; init; } = LoggerSetupFlags.Everything;

    /// <summary>
    /// Optional action invoked by the logging system during setup.
    /// </summary>
    /// <remarks>
    /// This action is only invoked if SimLynx is responsible for setting up the logging system
    /// (i.e., if <see cref="Setup"/> includes <see cref="LoggerSetupFlags.Setup"/>).
    /// </remarks>
    public Action<ILoggingBuilder>? ConfigureLogging { get; init; } = null;

    /// <summary>
    /// Flags for controlling what logging configuration SimLynx is responsible for.
    /// </summary>
    [Flags]
    public enum LoggerSetupFlags
    {
        /// <summary>
        /// SimLynx will not configure any logging other than registering a default no-op "null" logger.
        /// </summary>
        None = 0,

        /// <summary>
        /// SimLynx will fully configure logging, including setting up the logging system and configuring providers.
        /// </summary>
        /// <remarks>
        /// Logger are available to be injected into services via <see cref="ILogger"/> and
        /// <see cref="ILogger{TCategoryName}"/>.
        /// </remarks>
        Everything = ~0,

        /// <summary>
        /// SimLynx will configure all of its providers, but will not set up the logging system itself.
        /// </summary>
        /// <remarks>
        /// Useful if you want to configure the logging system yourself but want SimLynx's providers.
        /// </remarks>
        AllProviders = Everything & ~Setup,

        /// <summary>
        /// SimLynx will set up the logging system and enable injection of <see cref="ILogger"/> and
        /// <see cref="ILogger{TCategoryName}"/> into services.
        /// </summary>
        /// <remarks>
        /// This does not include any logging providers unless otherwise specified.
        /// </remarks>
        Setup = 1 << 0,

        /// <summary>
        /// SimLynx will configure a console logging provider that writes to the console (i.e. stdout/stderr)
        /// </summary>
        /// <remarks>
        /// Configures a <see cref="Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider"/> with the
        /// <see cref="Microsoft.Extensions.Logging.Console.SimpleConsoleFormatter"/>.
        /// </remarks>
        ConsoleProvider = 1 << 1,
    }
}
