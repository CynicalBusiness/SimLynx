using Autofac;
using SimLynx.Core;
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
        builder.RegisterType<PrototypeConfigSlotCatalog>().AsSelf().InstancePerDesign();

        builder.RegisterGeneric(typeof(PrototypeContext<>)).AsSelf().InstancePerDependency();
        builder.RegisterGeneric(typeof(PrototypeResolver<>)).AsSelf().InstancePerDependency();

        builder.RegisterPrototypeConfigSlot(
            new(PropertyConfigSlot.SLOT_ID, typeof(PropertyConfigSlot<>))
            {
                ConfigTypes = [typeof(IPropertyConfig), typeof(IPropertyConfig<>)],
                Priority = Priorities.High,
            }
        );
    }
}
