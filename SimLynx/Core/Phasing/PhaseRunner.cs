using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Executes phases in SimLynx, beginning from the current scope/phase.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="currentScope">The current scope to resolve dependencies from.</param>
public partial class PhaseRunner(ILogger<PhaseRunner>? logger, ILifetimeScope currentScope)
{
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
        LogEvent.PhasePlan(logger, phasePlan);

        if (
            !currentScope.TryResolve(typeof(PhaseBuilder<>).MakeGenericType(phaseInfo.PhaseType), out var rawBuilder)
            || rawBuilder is not IPhaseBuilder builder
        )
        {
            throw new InvalidOperationException($"No phase builder registered for phase: {phaseInfo.PhaseType}");
        }

        LogEvent.PhaseBegin(logger, phaseInfo.PhaseName);

        var phase = await builder.BuildAsync(cancellationToken);
        await using var nextScope = phase.Scope;

        try
        {
            await phase.RunAsync(cancellationToken);
            LogEvent.PhaseEnd(logger, phaseInfo.PhaseName);
        }
        catch (Exception ex)
        {
            LogEvent.PhaseError(logger, phaseInfo.PhaseName, ex);
            throw;
        }

        if (nextPhasePlan.Entries.Length == 0)
        {
            LogEvent.PhasePlanEnd(logger);
            return;
        }
        else
        {
            LogEvent.PhasePlanNext(logger, phaseInfo.PhaseName, nextPhasePlan.Entries[0].PhaseName);
            await nextScope.BeginPhase(nextPhasePlan, cancellationToken);
        }
    }

    /// <summary>
    /// <see cref="PhaseRunner"/> log events.
    /// </summary>
    public static partial class LogEvent
    {
        /// <summary>
        /// Event ID for a phase beginning.
        /// </summary>
        public const int PHASE_BEGIN = 1;

        [LoggerMessage(EventId = PHASE_BEGIN, Level = LogLevel.Information, Message = "== PHASE BEGIN: {phaseName}")]
        internal static partial void PhaseBegin(ILogger? logger, string phaseName);

        /// <summary>
        /// Event ID for a phase stopping or being cancelled.
        /// </summary>
        public const int PHASE_STOP = 2;

        [LoggerMessage(
            EventId = PHASE_STOP,
            Level = LogLevel.Information,
            Message = "Phase stopped/cancelled: {phaseName}"
        )]
        internal static partial void PhaseStop(ILogger? logger, string phaseName);

        /// <summary>
        /// Event ID for a phase error.
        /// </summary>
        public const int PHASE_ERROR = 3;

        [LoggerMessage(EventId = PHASE_ERROR, Level = LogLevel.Error, Message = "Exception in phase: {phaseName}")]
        internal static partial void PhaseError(ILogger? logger, string phaseName, Exception exception);

        /// <summary>
        /// Event ID for a phase completing execution.
        /// </summary>
        public const int PHASE_END = 4;

        [LoggerMessage(
            EventId = PHASE_END,
            Level = LogLevel.Information,
            Message = "Phase completed execution: {phaseName}"
        )]
        internal static partial void PhaseEnd(ILogger? logger, string phaseName);

        /// <summary>
        /// Event ID for a information about a phase plan.
        /// </summary>
        public const int PHASE_PLAN = 10;

        [LoggerMessage(EventId = PHASE_PLAN, Level = LogLevel.Debug, Message = "Executing phase plan: {phasePlan}")]
        internal static partial void PhasePlan(ILogger? logger, PhasePlan phasePlan);

        /// <summary>
        /// Event ID for the end of a phase plan.
        /// </summary>
        public const int PHASE_PLAN_END = 11;

        [LoggerMessage(EventId = PHASE_PLAN_END, Level = LogLevel.Information, Message = "End of phase plan reached.")]
        internal static partial void PhasePlanEnd(ILogger? logger);

        /// <summary>
        /// Event ID for advancing to the next phase in a phase plan.
        /// </summary>
        public const int PHASE_PLAN_NEXT = 12;

        [LoggerMessage(
            EventId = PHASE_PLAN_NEXT,
            Level = LogLevel.Information,
            Message = "Advancing to next phase in plan: {currentPhaseName} -> {nextPhaseName}"
        )]
        internal static partial void PhasePlanNext(ILogger? logger, string currentPhaseName, string nextPhaseName);
    }
}
