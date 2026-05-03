using SimLynx.Core;

namespace SimLynx.Simulation;

/// <summary>
/// Host for the SimLynx simulation, responsible for running the simulation loop.
/// </summary>
public interface ISimulationHost
{
    /// <summary>
    /// The metronome used for the simulation's update loop.
    /// </summary>
    public Metronome UpdateLoop { get; }
}
