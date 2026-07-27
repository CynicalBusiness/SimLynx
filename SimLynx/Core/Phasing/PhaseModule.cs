using Autofac;
using SimLynx.Core.Hooks;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Module for registering a phase and its manager in the DI container.
/// </summary>
/// <typeparam name="TPhase">The type of the phase.</typeparam>
public class PhaseModule<TPhase> : Module
    where TPhase : class, IPhase
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder
            .RegisterHook<OnPhaseConfigure<TPhase>>()
            .IfNotRegistered(typeof(Hook<OnPhaseConfigure<TPhase>>))
            .WithDeliveryStrategy(SerialHookDeliveryStrategy.Default) // the ConfigurationBuilder is not thread-safe, so we must run serially
            .WithPipe(payload => (OnPhaseConfigure)payload);
        builder
            .RegisterHook<OnPhaseInit<TPhase>>()
            .IfNotRegistered(typeof(Hook<OnPhaseInit<TPhase>>))
            .WithDeliveryStrategy(ConcurrentHookDeliveryStrategy.Default)
            .WithPipe(payload => (OnPhaseInit)payload);
        builder
            .RegisterHook<OnPhaseRun<TPhase>>()
            .IfNotRegistered(typeof(Hook<OnPhaseRun<TPhase>>))
            .WithDeliveryStrategy(ConcurrentHookDeliveryStrategy.Default)
            .WithPipe(payload => (OnPhaseRun)payload);

        builder.RegisterType<TPhase>().AsSelf().As<Phase>().AsImplementedInterfaces().InstancePerPhase(typeof(TPhase));
        builder.RegisterType<PhaseBuilder<TPhase>>().AsSelf().AsImplementedInterfaces().InstancePerDependency();
    }
}
