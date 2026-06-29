namespace SimLynx.Simulation.Systems;

/// <summary>
/// Generalized interface for a simulation system.
/// </summary>
/// <remarks>
/// Systems are per-<see cref="IStage">stage</see> services that operate broadly on entities based on their
/// defined components, regardless of their prototype heritage, much like systems in an ECS architecture.
/// </remarks>
public interface ISystem { }
