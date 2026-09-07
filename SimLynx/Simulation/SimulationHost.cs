using System;
using System.Threading.Tasks;
using SimLynx.Core;
using SimLynx.Core.Hooks;
using SimLynx.Core.Phasing;

namespace SimLynx.Simulation;

internal class SimulationHost : ISimulationService, IDisposable
{
    private readonly IDisposable subscription;

    public SimulationHost(IHookInvocable<OnSimulationUpdate> onSimUpdate)
    {
        subscription = UpdateLoop.PipeTo(onSimUpdate, tick => new OnSimulationUpdate(Delta: tick.Delta));
    }

    public Metronome UpdateLoop { get; } =
        new(SimulationPhase.DefaultTickRate, ConcurrentHookDeliveryStrategy.Default)
        {
            Name = SimulationPhase.METRONOME_NAME,
        };

    public void Dispose()
    {
        subscription.Dispose();
        UpdateLoop.Dispose();
    }

    public Task HandleHook(OnPhaseRun<SimulationPhase> payload, HookContext context)
    {
        context.CancellationToken.Register(() => UpdateLoop.Stop());
        return UpdateLoop.Start();
    }
}
