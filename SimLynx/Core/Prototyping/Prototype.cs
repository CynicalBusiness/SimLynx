namespace SimLynx.Core.Prototyping;

/// <summary>
/// Standard base implementation of a prototype.
/// </summary>
/// <remarks>
/// Contains default implementations and helpers suitable for most prototypes.
/// </remarks>
public abstract class Prototype : IPrototype
{
    /// <inheritdoc/>
    public required Symbol Id { get; init; }
}
