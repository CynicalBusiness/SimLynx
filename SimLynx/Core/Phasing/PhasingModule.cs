using Autofac;
using SimLynx.Core.Hooks;

namespace SimLynx.Core.Phasing;

internal class PhasingModule() : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterHook<OnPhaseConfigure>().WithDeliveryStrategy(SerialHookDeliveryStrategy.Default);
        builder.RegisterHook<OnPhaseInit>().WithDeliveryStrategy(ConcurrentHookDeliveryStrategy.Default);
        builder.RegisterHook<OnPhaseRun>().WithDeliveryStrategy(ConcurrentHookDeliveryStrategy.Default);

        builder.RegisterType<PhaseRunner>().InstancePerLifetimeScope();
    }
}
