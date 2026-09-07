namespace SimLynx.Core;

/// <summary>
/// Represents a dependency of a component on another component type.
/// </summary>
/// <typeparam name="T">The type of item key being depended on or resolved.</typeparam>
/// <param name="Key">The item key being depended on or resolved.</param>
/// <param name="Modes">The resolution modes for the dependency.</param>
public record class DependencyInfo<T>(T Key, DependencyResolutionModes Modes = default)
{
    /// <summary>
    /// Indicates whether the dependency is required to be satisfied.
    /// </summary>
    /// <seealso cref="DependencyResolutionModes.Required"/>
    public bool IsRequired
    {
        get => Modes.HasFlag(DependencyResolutionModes.Required);
        init =>
            Modes = value ? Modes | DependencyResolutionModes.Required : Modes & ~DependencyResolutionModes.Required;
    }

    /// <summary>
    /// Indicates whether the dependency must be satisfied exactly.
    /// </summary>
    /// <seealso cref="DependencyResolutionModes.Exact"/>
    public bool IsExact
    {
        get => Modes.HasFlag(DependencyResolutionModes.Exact);
        init => Modes = value ? Modes | DependencyResolutionModes.Exact : Modes & ~DependencyResolutionModes.Exact;
    }

    /// <summary>
    /// Indicates whether the dependency must be satisfied explicitly.
    /// </summary>
    /// <seealso cref="DependencyResolutionModes.Explicit"/>
    public bool IsExplicit
    {
        get => Modes.HasFlag(DependencyResolutionModes.Explicit);
        init =>
            Modes = value ? Modes | DependencyResolutionModes.Explicit : Modes & ~DependencyResolutionModes.Explicit;
    }
}
