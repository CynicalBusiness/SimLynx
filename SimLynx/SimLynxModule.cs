
using Autofac;
using Microsoft.Extensions.Logging;
using SimLynx.Core;
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

        builder.RegisterModule<LoggingModule>()
            .IfNotRegistered(typeof(ILogger));

        builder.RegisterType<TApp>()
            .AsSelf()
            .As<SimLynxApp>()
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.RegisterModule<DiscoveryModule>();
        builder.RegisterModule<DesignModule>();
        builder.RegisterModule<SimulationModule>();
    }
}
