using Autofac;
using SimLynx.Core.Prototyping.Properties;

namespace SimLynx.Core.Prototyping;

internal sealed class PrototypingModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterGeneric(typeof(PrototypeConfigSlotResolver<>)).AsSelf().InstancePerDependency();
        builder.RegisterGeneric(typeof(PrototypeContext<>)).AsSelf().InstancePerDependency();
        builder.RegisterType<PrototypeResolver>().AsSelf().InstancePerDependency();

        builder.RegisterModule(
            new PrototypeConfigModule(typeof(PropertyConfigSlot<>), PropertyConfigSlot.SLOT_ID)
            {
                ConfigTypes = [typeof(IPropertyConfig), typeof(IPropertyConfig<>), typeof(PropertyConfig<,>)],
            }
        );
    }
}
