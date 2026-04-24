using Autofac;
using SimLynx.Core.Phasing;

namespace SimLynx.Simulation;

internal class SimulationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterPhase<SimulationPhase>(SimulationPhase.PhaseId);
    }
}
