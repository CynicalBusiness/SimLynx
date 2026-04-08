namespace SimLynx.Core.Messaging;

/// <summary>
/// Constants representing message subscriber priority levels.
/// </summary>
public static class MessageSubscriberPriority
{
    /// <summary>
    /// Lowest priority level, executed <em>after all</em> others.
    /// </summary>
    public const sbyte LOWEST = -128;

    /// <summary>
    /// Lower priority level, executed <em>after most</em> others.
    /// </summary>
    public const sbyte LOWER = -64;

    /// <summary>
    /// Low priority level, executed <em>after</em> others.
    /// </summary>
    public const sbyte LOW = -32;

    /// <summary>
    /// Normal priority level.
    /// </summary>
    public const sbyte NORMAL = 0;

    /// <summary>
    /// High priority level, executed <em>before</em> others.
    /// </summary>
    public const sbyte HIGH = 32;

    /// <summary>
    /// Higher priority level, executed <em>before most</em> others.
    /// </summary>
    public const sbyte HIGHER = 64;

    /// <summary>
    /// Highest priority level, executed <em>before all</em> others.
    /// </summary>
    public const sbyte HIGHEST = 127;
}
