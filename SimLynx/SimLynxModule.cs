using Autofac;
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
/// <typeparam name="TApp">The type of the SimLynx application.</typeparam>
/// <param name="options">The options for configuring SimLynx.</param>
public class SimLynxModule<TApp>(SimLynxOptions options) : Module
    where TApp : SimLynxApp
{
    /// <summary>
    /// Creates a new SimLynx module with the default options.
    /// </summary>
    public SimLynxModule()
        : this(SimLynxOptions.Default) { }

    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        // core components
        builder.RegisterType<ParentLifetimeScopeAccessor>().AsSelf().InstancePerLifetimeScope();

        // core modules
        builder.RegisterModule(new LoggingModule(options.Logging));
        builder.RegisterModule<HooksModule>();
        builder.RegisterModule<PhasingModule>();

        // phase modules
        builder.RegisterModule<DiscoveryModule>();
        builder.RegisterModule<DesignModule>();
        builder.RegisterModule<SimulationModule>();

        // app
        builder.RegisterType<TApp>().AsSelf().As<SimLynxApp>().AsImplementedInterfaces().SingleInstance();
        builder.Register(c => c.Resolve<TApp>().CreatePhasePlan()).As<PhasePlan>().SingleInstance();
    }
}
