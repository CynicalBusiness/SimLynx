
using Autofac;
using SimLynx.Core;

namespace SimLynx.Design;

/// <summary>
/// The design register is responsible for handling all SimLynx design-time operations.
/// </summary>
public class DesignRegister : Register
{

    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<DesignHost>()
            .AsSelf()
            .SingleInstance();

        base.Load(builder);
    }

}
