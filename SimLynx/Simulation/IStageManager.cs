using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SimLynx.Core.Logging;

namespace SimLynx.Simulation;

/// <summary>
/// Manager for stages within the simulation phase.
/// </summary>
public interface IStageManager
{
    /// <summary>
    /// Log event: a stage was activated.
    /// </summary>
    public static LogEvent<string> StageActivatedLogEvent { get; } =
        new(EventId.For<IStageManager>("StageActivated"), "Activated stage: {StageName}", LogLevel.Information);

    /// <summary>
    /// Gets the stage with the given name, or null if no such stage exists.
    /// </summary>
    /// <param name="stageName">The name of the stage to retrieve.</param>
    /// <returns>The stage with the given name, or null if no such stage exists.</returns>
    public IStage? this[string stageName] { get; }

    /// <summary>
    /// Tries to get the stage with the given name.
    /// </summary>
    /// <param name="stageName">The name of the stage to retrieve.</param>
    /// <param name="stage">When this method returns, contains the stage with the given name, if it exists; otherwise, null.</param>
    /// <returns>true if the stage with the given name exists; otherwise, false.</returns>
    public bool TryGetStage(string stageName, out IStage? stage);

    /// <summary>
    /// Gets all stages currently in the simulation.
    /// </summary>
    /// <returns>An enumerable collection of all stages currently in the simulation.</returns>
    public IEnumerable<IStage> GetAllStages();
}
