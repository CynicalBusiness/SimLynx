using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Core;

namespace SimLynx.Design.Prototyping;

internal class PrototypeSubjectRegistrationSource<TBaseSubject>() : IRegistrationSource
    where TBaseSubject : class, IPrototypeSubject
{
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

        var registration = RegistrationBuilder
            .ForType(serviceType)
            .As(service)
            .InstancePerDependency()
            .CreateRegistration();

        return [registration];
    }
}
