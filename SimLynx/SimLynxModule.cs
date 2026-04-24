using Autofac;
using Microsoft.Extensions.Logging;
using SimLynx.Core;
using SimLynx.Core.Hooks;
using SimLynx.Core.Logging;
using SimLynx.Core.Phasing;
using SimLynx.Design;
using SimLynx.Discovery;
using SimLynx.Simulation;

namespace SimLynx;

/// <summary>
/// The main SimLynx Autofac module.
/// </summary>
public class SimLynxModule<TApp> : Module
    where TApp : SimLynxApp
{
    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        // core components
        builder.RegisterType<ParentLifetimeScopeAccessor>().AsSelf().InstancePerLifetimeScope();

        // core modules
        builder.RegisterModule<LoggingModule>().IfNotRegistered(typeof(ILogger));
        builder.RegisterModule<HooksModule>();

        // phase modules
        builder.RegisterModule<PhaseModule>();
        builder.RegisterModule<DiscoveryModule>();
        builder.RegisterModule<DesignModule>();
        builder.RegisterModule<SimulationModule>();

        builder.RegisterType<TApp>().AsSelf().As<SimLynxApp>().AsImplementedInterfaces().SingleInstance();
    }
}
