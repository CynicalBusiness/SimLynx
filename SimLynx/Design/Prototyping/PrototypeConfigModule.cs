using Autofac;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Module which registers a prototype configuration slot type.
/// </summary>
public class PrototypeConfigModule(PrototypeConfigSlotDef options) : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(options).AsSelf();

        // only register the actual type def; lookup by config type is the catalog's responsibility
        builder
            .RegisterGeneric(options.TypeDef)
            .Keyed(options.Id, typeof(IPrototypeConfigSlotFor<>))
            .InstancePerDependency();
    }
}
