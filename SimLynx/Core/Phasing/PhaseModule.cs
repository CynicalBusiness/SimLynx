using Autofac;
using SimLynx.Core.Hooks;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Module for registering a phase and its manager in the DI container.
/// </summary>
/// <typeparam name="TPhase">The type of the phase.</typeparam>
/// <param name="phaseId">The identifier for the phase.</param>
public class PhaseModule<TPhase>(string phaseId) : Module
    where TPhase : class, IPhase
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder
            .RegisterHook<OnPhaseConfigure<TPhase>>()
            .IfNotRegistered(typeof(Hook<OnPhaseConfigure<TPhase>>))
            .WithPipe(payload => (OnPhaseConfigure)payload)
            .WithDeliveryStrategy(SerialHookDeliveryStrategy.Default); // the ConfigurationBuilder is not thread-safe, so we must run serially
        builder
            .RegisterHook<OnPhaseInit<TPhase>>()
            .IfNotRegistered(typeof(Hook<OnPhaseInit<TPhase>>))
            .WithPipe(payload => (OnPhaseInit)payload)
            .WithDeliveryStrategy(ConcurrentHookDeliveryStrategy.Default);
        builder
            .RegisterHook<OnPhaseRun<TPhase>>()
            .IfNotRegistered(typeof(Hook<OnPhaseRun<TPhase>>))
            .WithPipe(payload => (OnPhaseRun)payload)
            .WithDeliveryStrategy(ConcurrentHookDeliveryStrategy.Default);

        builder
            .RegisterType<TPhase>()
            .AsSelf()
            .AsImplementedInterfaces()
            .Named<TPhase>(phaseId)
            .InstancePerOwned<TPhase>();

        builder
            .RegisterType<PhaseBuilder<TPhase>>()
            .AsSelf()
            .AsImplementedInterfaces()
            .WithParameter(new NamedParameter("phaseId", phaseId))
            .Named<PhaseBuilder<TPhase>>(phaseId)
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

        builder.RegisterHook<OnPhaseConfigure>().WithDeliveryStrategy(SerialHookDeliveryStrategy.Default);
        builder.RegisterHook<OnPhaseInit>().WithDeliveryStrategy(ConcurrentHookDeliveryStrategy.Default);
        builder.RegisterHook<OnPhaseRun>().WithDeliveryStrategy(ConcurrentHookDeliveryStrategy.Default);

        builder.RegisterType<PhaseManager>().As<IPhaseManager>().InstancePerLifetimeScope();
    }
}
