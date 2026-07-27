using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using SimLynx.Core.Logging;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Executes phases in SimLynx, beginning from the current scope/phase.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="currentScope">The current scope to resolve dependencies from.</param>
public class PhaseRunner(ILogger<PhaseRunner> logger, ILifetimeScope currentScope)
{
    internal static readonly LogEvent<string> PhaseStartLogEvent = new(
        EventId.For<PhaseRunner>("PhaseStart"),
        "** BEGIN PHASE: {Phase}"
    );
    internal static readonly LogEvent<string> PhaseEndLogEvent = new(
        EventId.For<PhaseRunner>("PhaseEnd"),
        "** END PHASE: {Phase}"
    );
    internal static readonly LogEvent<string> PhaseExceptionLogEvent = new(
        EventId.For<PhaseRunner>("PhaseException"),
        "** EXCEPTION IN PHASE: {Phase}",
        LogLevel.Error
    );
    internal static readonly LogEvent<string, string> NextPhaseLogEvent = new(
        EventId.For<PhaseRunner>("NextPhase"),
        "Trying to start next phase: {CurrentPhase} -> {Phase}",
        LogLevel.Debug
    );
    internal static readonly LogEvent EndOfPhasePlanLogEvent = new(
        EventId.For<PhaseRunner>("EndOfPhasePlan"),
        "Reached end of phase plan, nothing more to do.",
        LogLevel.Information
    );

    /// <summary>
    /// Begins the execution of the provided <paramref name="phasePlan"/>.
    /// </summary>
    /// <param name="phasePlan">The phase plan to execute.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to abort the process.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task StartAsync(PhasePlan phasePlan, CancellationToken cancellationToken = default)
    {
        if (!phasePlan.TryNext(out var phaseInfo, out var nextPhasePlan))
        {
            return;
        }

        if (
            !currentScope.TryResolve(typeof(PhaseBuilder<>).MakeGenericType(phaseInfo.PhaseType), out var rawBuilder)
            || rawBuilder is not IPhaseBuilder builder
        )
        {
            throw new InvalidOperationException($"No phase builder registered for phase: {phaseInfo.PhaseType}");
        }

        PhaseStartLogEvent.Log(logger, phaseInfo.PhaseName);

        var phase = await builder.BuildAsync(cancellationToken);
        await using var nextScope = phase.Scope;

        try
        {
            await phase.RunAsync(cancellationToken);
            PhaseEndLogEvent.Log(logger, phaseInfo.PhaseName);
        }
        catch (Exception ex)
        {
            PhaseExceptionLogEvent.Log(logger, phaseInfo.PhaseName, ex);
            throw;
        }

        if (nextPhasePlan.Entries.Length == 0)
        {
            EndOfPhasePlanLogEvent.Log(logger);
            return;
        }
        else
        {
            NextPhaseLogEvent.Log(logger, phaseInfo.PhaseName, nextPhasePlan.Entries[0].PhaseName);
            await nextScope.BeginPhase(nextPhasePlan, cancellationToken);
        }
    }
}
