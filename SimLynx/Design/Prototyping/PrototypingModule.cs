using Autofac;
using SimLynx.Design.Prototyping.Properties;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Module for registering the prototyping system with an Autofac container.
/// </summary>
public sealed class PrototypingModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterGeneric(typeof(PrototypeConfigSlotResolver<>)).AsSelf().InstancePerDependency();
        builder.RegisterGeneric(typeof(PrototypeContext<>)).AsSelf().InstancePerDependency();
        builder.RegisterGeneric(typeof(PrototypeResolver<>)).AsSelf().InstancePerDependency();

        builder.RegisterModule(
            new PrototypeConfigModule(typeof(PropertyConfigSlot<>), PropertyConfigSlot.SLOT_ID)
            {
                ConfigTypes = [typeof(IPropertyConfig), typeof(IPropertyConfig<>), typeof(PropertyConfig<,>)],
            }
        );
    }
}
