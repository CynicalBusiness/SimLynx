using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Resolver for prototypes.
/// </summary>
/// <param name="container">The component context used to resolve prototypes.</param>
public class PrototypeResolver<TBaseSubject>(IComponentContext container)
    where TBaseSubject : class, IPrototypeSubject
{
    /// <summary>
    /// Resolves a prototype with the given <typeparamref name="TSubject"/> subject type, setting
    /// <paramref name="isAbstract"/> and <paramref name="basePrototype"/> if provided.
    /// </summary>
    /// <typeparam name="TSubject">The type of the subject for the prototype.</typeparam>
    /// <param name="id">The ID of the prototype to resolve.</param>
    /// <param name="isAbstract">Indicates whether the prototype is abstract.</param>
    /// <param name="basePrototype">The base prototype, if any.</param>
    /// <returns>The resolved prototype.</returns>
    public IPrototype<TSubject> Resolve<TSubject>(Symbol id, Maybe<bool> isAbstract, Maybe<IPrototype?> basePrototype)
        where TSubject : class, TBaseSubject
    {
        IEnumerable<Parameter> parameters = [new TypedParameter(typeof(Symbol), id)];
        if (isAbstract.HasValue)
        {
            parameters = parameters.Append(new NamedPropertyParameter(nameof(IPrototype.IsAbstract), isAbstract.Value));
        }
        if (basePrototype.HasValue)
        {
            parameters = parameters.Append(new NamedPropertyParameter(nameof(IPrototype.Base), basePrototype.Value));
        }

        return container.ResolveKeyed<IPrototype<TSubject>>(typeof(TBaseSubject), parameters);
    }
}
