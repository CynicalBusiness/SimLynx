using Autofac;
using SimLynx.Core.Phasing;

namespace SimLynx.Design;

internal class DesignModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterPhase<DesignPhase, DesignPhase.Builder>(DesignPhase.PhaseId);
    }
}
