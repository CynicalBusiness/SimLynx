using System;
using Autofac;
using SimLynx.Design;
using SimLynx.Simulation;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// Module to register prototyping services for a particular subject type.
/// </summary>
/// <typeparam name="TBaseSubject">The base type of the subject for which this module is registering prototyping services.</typeparam>
public class PrototypeModule<TBaseSubject> : Module
    where TBaseSubject : class, IPrototypeSubject
{
    /// <summary>
    /// The concrete prototype, as a generic type definition.
    /// </summary>
    public Type PrototypeImpl
    {
        get;
        init
        {
            if (!value.IsGenericTypeDefinition || !value.IsAssignableTo(typeof(IPrototype<>)))
            {
                throw new ArgumentException(
                    $"The provided type '{value}' must be a generic type definition that implements IPrototype<>."
                );
            }

            try
            {
                value.MakeGenericType(typeof(TBaseSubject));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    $"The provided type '{value}' must have exactly one generic type argument that is assignable from {typeof(TBaseSubject)}.",
                    ex
                );
            }

            field = value;
        }
    } = typeof(Prototype<>);

    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterSource(new PrototypeSubjectRegistrationSource<TBaseSubject>());

        builder.RegisterType<PrototypeRegistry<TBaseSubject>>().AsSelf().InstancePerDesign();
        builder.Register(c => c.Resolve<PrototypeRegistry<TBaseSubject>>().Compile()).AsSelf().InstancePerSimulation();

        builder
            .RegisterGeneric(PrototypeImpl)
            .Keyed(typeof(TBaseSubject), typeof(IPrototype<>))
            .PropertiesAutowired(PrototypePropertySelector.Instance)
            .InstancePerDependency();
    }
}
