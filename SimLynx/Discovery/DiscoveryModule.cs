using Autofac;
using SimLynx.Core.Phasing;

namespace SimLynx.Discovery;

internal class DiscoveryModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        builder.RegisterPhase<DiscoveryPhase>(DiscoveryPhase.PhaseId);
    }
}
