
using Autofac;
using Microsoft.Extensions.Logging;
using SimLynx.Core;
using SimLynx.Core.States;
using SimLynx.Design;

namespace SimLynx;

/// <summary>
/// The main SimLynx Autofac module.
/// </summary>
public class SimLynxModule : Module
{
    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SimLynxHost>()
            .AsSelf()
            .SingleInstance();

        // states
        builder.RegisterGeneric(typeof(StateType<>))
            .AsSelf()
            .SingleInstance();
        builder.RegisterType<StateTypeResolver>()
            .AsSelf()
            .SingleInstance();

        // design
        builder.RegisterType<DesignRegister>()
            .AsSelf()
            .SingleInstance();

        // runtime
        builder.RegisterType<RuntimeRegister>()
            .AsSelf()
            .SingleInstance();

        base.Load(builder);
    }
}
