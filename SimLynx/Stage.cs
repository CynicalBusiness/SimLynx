namespace SimLynx;

/// <summary>
/// The root unit of simulation within SimLynx, a Stage (like a stage in a theater) represents a single "world" in which
/// entities interact.
/// </summary>
public class Stage(Symbol name)
{
    /// <summary>
    /// The name of the stage.
    /// </summary>
    public Symbol Name { get; } = name;
}
