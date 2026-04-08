using Autofac;

namespace SimLynx.Core.Messaging;

internal class MessagingModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<ScopedMessageBus>().As<IMessageBus>().InstancePerLifetimeScope();
    }
}
