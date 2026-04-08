using Autofac;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Module for registering a phase and its manager in the DI container.
/// </summary>
/// <typeparam name="TPhase">The type of the phase.</typeparam>
/// <typeparam name="TPhaseManager">The type of the phase manager.</typeparam>
/// <param name="phaseId">The identifier for the phase.</param>
public class PhaseModule<TPhase, TPhaseManager>(string phaseId) : Module
    where TPhase : class, IPhase
    where TPhaseManager : class, IPhaseBuilder<TPhase>
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder
            .RegisterType<TPhase>()
            .AsSelf()
            .AsImplementedInterfaces()
            .Named<TPhase>(phaseId)
            .InstancePerOwned<TPhase>();

        builder
            .RegisterType<TPhaseManager>()
            .AsSelf()
            .AsImplementedInterfaces()
            .Named<TPhaseManager>(phaseId)
            .InstancePerDependency();
    }
}

/// <summary>
/// Module to register general phase-related infrastructure in the DI container.
/// </summary>
internal class PhaseModule() : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<PhaseManager>().As<IPhaseManager>().InstancePerLifetimeScope();
    }
}
