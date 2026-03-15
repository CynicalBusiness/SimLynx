
using Autofac;
using SimLynx.Core.Phasing;

namespace SimLynx.Discovery;

internal class DiscoveryModule : Module
{

    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<DiscoveryPhaseManager>()
            .AsSelf()
            .As<IPhaseManager>()
            .IdentifiedBy(DiscoveryPhase.PhaseId)
            .InstancePerLifetimeScope();
    }

}
