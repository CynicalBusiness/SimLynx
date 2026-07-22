using Autofac;
using SimLynx.Design.Prototyping;
using SimLynx.Simulation.ComponentModel.Prototyping;

namespace SimLynx.Simulation.ComponentModel;

internal class ComponentModelModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule(new PrototypeModule<Component>() { PrototypeImpl = typeof(ComponentPrototype<>) });
    }
}
