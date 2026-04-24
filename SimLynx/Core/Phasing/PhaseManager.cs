using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Microsoft.Extensions.Logging;
using SimLynx.Core.Logging;

namespace SimLynx.Core.Phasing;

internal class PhaseManager(ILogger<PhaseManager> logger, IIndex<string, IPhaseBuilder> phaseBuilders) : IPhaseManager
{
    public static readonly LogEvent<string> PhaseStartLogEvent = new(
        EventId.For<PhaseManager>("PhaseStart"),
        "** BEGIN PHASE: {PhaseId}"
    );
    public static readonly LogEvent<string> PhaseEndLogEvent = new(
        EventId.For<PhaseManager>("PhaseEnd"),
        "** END PHASE: {PhaseId}"
    );
    public static readonly LogEvent<string> PhaseExceptionLogEvent = new(
        EventId.For<PhaseManager>("PhaseException"),
        "** EXCEPTION IN PHASE: {PhaseId}",
        LogLevel.Error
    );
    public static readonly LogEvent<string, string> NextPhaseLogEvent = new(
        EventId.For<PhaseManager>("NextPhase"),
        "Trying to start next phase: {CurrentPhaseId} -> {PhaseId}",
        LogLevel.Debug
    );

    public async Task StartAsync(string phaseId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phaseId, nameof(phaseId));

        if (!phaseBuilders.TryGetValue(phaseId, out var builder))
        {
            throw new ArgumentException($"No phase builder registered for phase ID: {phaseId}", nameof(phaseId));
        }

        var phase = await builder.BuildAsync(cancellationToken);
        await using var scope = phase.Scope;

        PhaseStartLogEvent.Log(logger, phaseId);
        try
        {
            await phase.RunAsync(cancellationToken);
            PhaseEndLogEvent.Log(logger, phaseId);
        }
        catch (Exception ex)
        {
            PhaseExceptionLogEvent.Log(logger, phaseId, ex);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(phase.NextPhaseId))
        {
            NextPhaseLogEvent.Log(logger, phaseId, phase.NextPhaseId);
            await scope.BeginPhase(phase.NextPhaseId, cancellationToken);
        }
    }
}
