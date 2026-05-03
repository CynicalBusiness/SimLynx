using Autofac;
using Microsoft.Extensions.Logging;
using SimLynx.Core.Hooks;
using SimLynx.Core.Phasing;

namespace SimLynx.Simulation;

internal class SimulationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterPhase<SimulationPhase>(SimulationPhase.PhaseId);

        builder
            .RegisterHook<OnSimulationUpdate>()
            .SimulationInstance()
            .WithDeliveryStrategy(ConcurrentHookDeliveryStrategy.Default);

        builder
            .RegisterType<SimulationHost>()
            .AsImplementedInterfaces()
            .SimulationInstance()
            .OnHook(e => new HookHandler<OnPhaseRun<SimulationPhase>>(e.Instance.HandleHook, e.Instance));
    }
}
