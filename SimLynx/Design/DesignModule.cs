using Autofac;
using SimLynx.Core.Phasing;
using SimLynx.Design.Prototyping;

namespace SimLynx.Design;

internal class DesignModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterPhase<DesignPhase>();

        builder.RegisterModule<PrototypingModule>();
    }
}
