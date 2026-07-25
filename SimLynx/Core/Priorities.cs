namespace SimLynx.Core;

/// <summary>
/// Constants for various priority levels used by SimLynx.
/// </summary>
/// <remarks>
/// Consumers are free to use these constants as well for other purposes.
/// <br/>
/// These priority levels are designed such that each bit of the <see langword="sbyte"/> that is set represents
/// additional priority away from the default of <c>0</c>, with the "sign" bit flipping this behavior for negative
/// values representing lesser priority as they grow.
/// <br/>
/// Priorities can easily be compared with standard <c>&gt;</c> and <c>&lt;</c> operators, as well as be combined via
/// either numeric addition (<c>+</c>) or bitwise OR (<c>|</c>) to create custom priorities mid-way between constants.
/// Note that these two methods of combination do not always yield the same result, as addition can "carry" bits over
/// to the next level.
/// </remarks>
public static class Priorities
{
    /// <summary>
    /// The default "normal" priority level.
    /// </summary>
    public const sbyte Default = 0;

    /// <summary>
    /// A high priority level, above the default.
    /// </summary>
    public const sbyte High = 0b000_0001;

    /// <summary>
    /// A higher priority level, above <see cref="High"/>.
    /// </summary>
    public const sbyte Higher = 0b000_1000;

    /// <summary>
    /// A very high priority level, above <see cref="Higher"/>.
    /// </summary>
    public const sbyte VeryHigh = 0b100_0000;

    /// <summary>
    /// A low priority level, below the default.
    /// </summary>
    public const sbyte Low = -High;

    /// <summary>
    /// A lower priority level, below <see cref="Low"/>.
    /// </summary>
    public const sbyte Lower = -Higher;

    /// <summary>
    /// A very low priority level, below <see cref="Lower"/>.
    /// </summary>
    public const sbyte VeryLow = -VeryHigh;

    /// <summary>
    /// The maximum possible priority level, above which the value would overflow.
    /// </summary>
    public const sbyte Max = sbyte.MaxValue;

    /// <summary>
    /// The minimum possible priority level, below which the value would underflow.
    /// </summary>
    public const sbyte Min = sbyte.MinValue;
}
