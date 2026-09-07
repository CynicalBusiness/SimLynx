using Autofac;
using SimLynx.Core;
using SimLynx.Design.Prototyping.Parameters;
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
        builder.RegisterType<PrototypeConfigSlotCatalog>().AsSelf().DesignInstance();
        builder.RegisterType<PrototypeContext>().AsSelf().InstancePerDependency();
        builder.RegisterGeneric(typeof(PrototypeResolver<>)).AsSelf().InstancePerDependency();

        builder.RegisterPrototypeConfigSlot(
            new(PropertyConfigSlot.SlotId, typeof(PropertyConfigSlot<>))
            {
                ConfigTypes = [typeof(IPropertyConfig), typeof(IPropertyConfig<>)],
                Priority = Priorities.High,
                Eager = true,
            }
        );

        builder.RegisterPrototypeConfigSlot(
            new(ParameterConfigSlot.SlotId, typeof(ParameterConfigSlot<>))
            {
                ConfigTypes = [typeof(IParameterConfig), typeof(IParameterConfig<>)],
            }
        );
    }
}
