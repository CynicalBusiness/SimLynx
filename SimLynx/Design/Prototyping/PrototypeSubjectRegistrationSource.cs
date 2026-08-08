using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Core;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Static helpers for handling subject registrations with Autofac.
/// </summary>
public static class PrototypeSubjectRegistrationSource
{
    /// <summary>
    /// Handler delegate for subject registration events.
    /// </summary>
    /// <param name="registration"></param>
    public delegate void SubjectRegistrationHandler(
        IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration
    );

    /// <summary>
    /// Event raised when a registration is created for any subject.
    /// </summary>
    public static event SubjectRegistrationHandler? OnRegisterSubject;

    internal static void ApplySubjectRegistrations(
        IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration
    )
    {
        OnRegisterSubject?.Invoke(registration);
    }
}

/// <summary>
/// A registration source that automatically provides registrations for prototype subject types deriving from
/// <typeparamref name="TBaseSubject"/>.
/// </summary>
/// <typeparam name="TBaseSubject">The base type of the prototype subject.</typeparam>
public class PrototypeSubjectRegistrationSource<TBaseSubject>() : IRegistrationSource
    where TBaseSubject : class, IPrototypeSubject
{
    /// <summary>
    /// Event raised when a registration is created for a subject.
    /// </summary>
    public PrototypeSubjectRegistrationSource.SubjectRegistrationHandler? OnRegisterSubject { get; init; }

    /// <inheritdoc/>
    public bool IsAdapterForIndividualComponents => false;

    /// <inheritdoc/>
    public IEnumerable<IComponentRegistration> RegistrationsFor(
        Service service,
        Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor
    )
    {
        if (service is not IServiceWithType swt)
        {
            return [];
        }

        var serviceType = swt.ServiceType;
        if (!serviceType.IsClass || serviceType.IsAbstract || !serviceType.IsAssignableTo(typeof(TBaseSubject)))
        {
            return [];
        }

        var builder = RegistrationBuilder.ForType(serviceType).As(service).InstancePerDependency();

        OnRegisterSubject?.Invoke(builder);
        PrototypeSubjectRegistrationSource.ApplySubjectRegistrations(builder);

        return [builder.CreateRegistration()];
    }
}
