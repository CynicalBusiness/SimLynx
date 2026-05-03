using System;

namespace SimLynx.Simulation;

/// <summary>
/// Hook for a simulation update.
/// </summary>
/// <param name="Delta">The time elapsed since the last update.</param>
public record OnSimulationUpdate(TimeSpan Delta);
