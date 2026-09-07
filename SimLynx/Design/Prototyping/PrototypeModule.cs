using System;
using Autofac;
using SimLynx.Core.Hooks;
using SimLynx.Design.Prototyping.Blueprints;
using SimLynx.Simulation;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Module to register prototyping services for a particular subject type.
/// </summary>
/// <typeparam name="TBaseSubject">The base type of the subject for which this module is registering prototyping services.</typeparam>
public class PrototypeModule<TBaseSubject> : Module
    where TBaseSubject : class, IPrototypeSubject
{
    private static readonly string[] InjectableProps = [nameof(IPrototype.Base), nameof(IPrototype.IsAbstract)];

    /// <summary>
    /// The concrete prototype, as a generic type definition.
    /// </summary>
    public Type PrototypeImpl
    {
        get;
        init
        {
            if (!value.IsGenericTypeDefinition || !value.IsGenericTypeOf(typeof(IPrototype<>)))
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

    /// <summary>
    /// Handler to apply additional configuration to subject registrations for this subject type.
    /// </summary>
    public PrototypeSubjectRegistrationSource.SubjectRegistrationHandler? OnRegisterSubject { get; init; }

    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterSource(
            new PrototypeSubjectRegistrationSource<TBaseSubject>() { OnRegisterSubject = OnRegisterSubject }
        );

        builder
            .RegisterType<PrototypeRegistry<TBaseSubject>>()
            .Named<PrototypeRegistry<TBaseSubject>>(PrototypeRegistry<TBaseSubject>.Factory.TRANSIENT_REGISTRY_NAME);
        builder.RegisterType<PrototypeRegistry<TBaseSubject>.Factory>().AsSelf();

        // singleton-ish default instances for registries and compiled catalogs
        builder.Register(c => c.Resolve<PrototypeRegistry<TBaseSubject>.Factory>().Create()).AsSelf().DesignInstance();
        builder.Register(c => c.Resolve<PrototypeRegistry<TBaseSubject>>().Compile()).AsSelf().InstancePerSimulation();

        builder.RegisterType<PrototypeResolver<TBaseSubject>>().AsSelf();
        builder
            .RegisterGeneric(PrototypeImpl)
            .Keyed(typeof(TBaseSubject), typeof(IPrototype<>))
            .PropertiesParameterized(InjectableProps)
            .InstancePerDependency();

        // blueprints
        builder.RegisterHook<OnCreateInstance>();
        builder.RegisterHook<OnBeforeCreateInstance>();
    }
}
