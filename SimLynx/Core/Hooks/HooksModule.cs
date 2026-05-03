using Autofac;

namespace SimLynx.Core.Hooks;

internal class HooksModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterInstance(ConcurrentHookDeliveryStrategy.Default).AsSelf();
        builder.RegisterInstance(SerialHookDeliveryStrategy.Default).AsSelf().As<IHookDeliveryStrategy>();
    }
}
