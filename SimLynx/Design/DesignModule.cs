using Autofac;
using SimLynx.Core.Phasing;

namespace SimLynx.Design;

internal class DesignModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder
            .RegisterType<DesignPhaseManager>()
            .AsSelf()
            .As<IPhaseManager>()
            .IdentifiedBy(DesignPhase.PhaseId)
            .InstancePerLifetimeScope();
    }
}
