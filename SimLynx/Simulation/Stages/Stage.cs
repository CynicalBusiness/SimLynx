using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimLynx.Core.Hooks;
using SimLynx.Core.Logging;

namespace SimLynx.Simulation.Stages;

/// <summary>
/// Basic implementation for a <see cref="IStage"/>.
/// </summary>
public abstract class Stage(string name, ILogger<Stage> logger) : IStage
{
    /// <summary>
    /// Log event for when the stage falls behind and has to jump ahead.
    /// </summary>
    public static readonly LogEvent<string, TimeSpan> JumpAheadLogEvent = new(
        EventId.For<Stage>("JumpAhead"),
        "Stage '{Name}' fell behind, jumped {Delta} ahead",
        LogLevel.Warning
    );

    /// <summary>
    /// Log event for when the stage is ahead and has to jump back
    /// </summary>
    public static readonly LogEvent<string, TimeSpan> JumpBackLogEvent = new(
        EventId.For<Stage>("JumpBack"),
        "Stage '{Name}' ahead of current time, jumping {Delta} back",
        LogLevel.Warning
    );

    private TimeSpan _updateSkipThreshold = TimeSpan.FromSeconds(1);

    /// <inheritdoc/>
    public string Name { get; } = name;

    /// <summary>
    /// The current time of the stage.
    /// </summary>
    public DateTime StageTime { get; private set; } = DateTime.Now;

    /// <summary>
    /// The threshold at which the stage will skip ahead instead of trying to catch up on updates.
    /// </summary>
    public TimeSpan UpdateSkipThreshold
    {
        get => _updateSkipThreshold;
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(UpdateSkipThreshold),
                    "Update skip threshold must be greater than zero"
                );
            }
            _updateSkipThreshold = value;
        }
    }

    /// <summary>
    /// Hook that triggers a simulation update.
    /// </summary>
    public required Hook<OnSimulationUpdate> OnSimulationUpdate { get; init; }

    /// <summary>
    /// Triggers a simulation update for this stage.
    /// </summary>
    /// <returns>A task that represents the update.</returns>
    public virtual Task UpdateAsync()
    {
        lock (this)
        {
            var now = DateTime.Now;
            var delta = now - StageTime;
            if (delta < TimeSpan.Zero)
            {
                // we're ahead of the current time, jump back
                JumpBackLogEvent.Log(logger, Name, delta);
                delta = TimeSpan.Zero;
            }
            else if (delta > UpdateSkipThreshold)
            {
                // we're too far behind, jump ahead
                JumpAheadLogEvent.Log(logger, Name, delta);
                delta = UpdateSkipThreshold;
            }

            StageTime = now;
            return OnSimulationUpdate.Invoke(new OnSimulationUpdate(delta));
        }
    }

    /// <inheritdoc/>
    public abstract Task RunAsync(CancellationToken cancellationToken);
}
