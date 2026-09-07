using SimLynx.Core;

namespace SimLynx.Simulation;

/// <summary>
/// Primary service for the operation of SimLynx's simulation, responsible for running the simulation loop.
/// </summary>
public interface ISimulationService
{
    /// <summary>
    /// The metronome used for the simulation's update loop.
    /// </summary>
    public Metronome UpdateLoop { get; }
}
