using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.Indexed;

namespace SimLynx.Core.Phasing;

internal class PhaseManager(IIndex<Symbol, IPhaseBuilder> phaseBuilders) : IPhaseManager
{
    public async Task StartAsync(Symbol phaseId, CancellationToken cancellationToken = default)
    {
        if (!phaseBuilders.TryGetValue(phaseId, out var builder))
        {
            throw new ArgumentException($"No phase builder registered for phase ID: {phaseId}", nameof(phaseId));
        }

        var phase = await builder.BuildAsync(cancellationToken);
        await using var scope = phase.Scope;

        await phase.RunAsync(cancellationToken);

        if (phase.NextPhaseId is not null)
        {
            await scope.BeginPhase(phase.NextPhaseId.Value, cancellationToken);
        }
    }
}
