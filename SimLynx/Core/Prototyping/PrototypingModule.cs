using Autofac;
using SimLynx.Core.Prototyping.Properties;

namespace SimLynx.Core.Prototyping;

internal sealed class PrototypingModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterPrototypeConfigResolver<PrototypePropertyConfigResolver>(
            PrototypePropertyConfigResolver.PROPERTIES_SLOT_NAME
        );
    }
}
