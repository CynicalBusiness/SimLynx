using Autofac;
using SimLynx.Core.Hooks;
using SimLynx.Core.Phasing;

namespace SimLynx.Simulation;

internal class SimulationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterPhase<SimulationPhase>();

        builder
            .RegisterHook<OnSimulationUpdate>()
            .InstancePerSimulation()
            .WithDeliveryStrategy(ConcurrentHookDeliveryStrategy.Default);

        builder
            .RegisterType<SimulationHost>()
            .AsImplementedInterfaces()
            .InstancePerSimulation()
            .OnHook(e => new HookHandler<OnPhaseRun<SimulationPhase>>(e.Instance.HandleHook, e.Instance));
        builder.RegisterType<StageManager>().AsImplementedInterfaces().InstancePerSimulation();

        builder.RegisterStage<Stage>(SimulationPhase.MAIN_STAGE_NAME).AsImplementedInterfaces();
        builder.Register(e => e.ResolveNamed<IStage>(SimulationPhase.MAIN_STAGE_NAME)).AsSelf();
    }
}
