namespace SimLynx.Simulation;

/// <summary>
/// Interface for stages within the simulation.
/// </summary>
/// <remarks>
/// Stages, as in "stages of a theater", are the top-level of the simulation, working similarly to "scenes" in other
/// game engines. Stages run completely independently and concurrently with respect to each-other, each with their
/// own set of components and systems.
/// </remarks>
public interface IStage
{
    /// <summary>
    /// The stage's name.
    /// </summary>
    public string Name { get; }
}
