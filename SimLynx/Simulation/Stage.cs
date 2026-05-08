using Autofac;
using SimLynx.Core.Hooks;

namespace SimLynx.Simulation;

/// <summary>
/// Basic implementation for a <see cref="IStage"/>.
/// </summary>
public class Stage([ServiceKey] string name) : IStage
{
    /// <inheritdoc/>
    public string Name { get; } = name;

    /// <summary>
    /// Hook that triggers a simulation update.
    /// </summary>
    public required Hook<OnSimulationUpdate> OnSimulationUpdate { get; init; }
}
