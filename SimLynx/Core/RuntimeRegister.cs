
using Autofac;

namespace SimLynx.Core;

/// <summary>
/// The runtime register is responsible for handling all runtime operations of SimLynx.
/// </summary>
public class RuntimeRegister(
    StageRegister stageRegister,
    PrototypeRegister prototypeRegister)
        : Register
{

    /// <summary>
    /// Register for stages
    /// </summary>
    public StageRegister Stages { get; } = stageRegister;

    /// <summary>
    /// Register for prototypes
    /// </summary>
    public PrototypeRegister Prototypes { get; } = prototypeRegister;

    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<RuntimeHost>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterModule(Stages);
        builder.RegisterModule(Prototypes);

        base.Load(builder);
    }

}
