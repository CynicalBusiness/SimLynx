using SimLynx.Core.Logging;

namespace SimLynx;

/// <summary>
/// Options for configuring SimLynx.
/// </summary>
public record class SimLynxOptions
{
    /// <summary>
    /// The default SimLynx options.
    /// </summary>
    public static SimLynxOptions Default { get; } = new();

    /// <summary>
    /// Options for how logging is configured.
    /// </summary>
    public LoggingOptions Logging { get; init; } = LoggingOptions.Default;
}
