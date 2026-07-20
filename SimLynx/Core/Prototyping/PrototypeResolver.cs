using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// Resolver for prototypes.
/// </summary>
/// <param name="container">The component context used to resolve prototypes.</param>
public class PrototypeResolver(IComponentContext container)
{
    /// <summary>
    /// Resolves a prototype with the given <typeparamref name="TSubject"/> subject type, setting
    /// <paramref name="isAbstract"/> and <paramref name="basePrototype"/> if provided.
    /// </summary>
    /// <typeparam name="TSubject">The type of the subject for the prototype.</typeparam>
    /// <param name="isAbstract">Indicates whether the prototype is abstract.</param>
    /// <param name="basePrototype">The base prototype, if any.</param>
    /// <returns>The resolved prototype.</returns>
    public IPrototype<TSubject> Resolve<TSubject>(Maybe<bool> isAbstract, Maybe<IPrototype?> basePrototype)
        where TSubject : class, IPrototypeSubject
    {
        IEnumerable<Parameter> parameters = [];
        if (isAbstract.HasValue)
        {
            parameters = parameters.Append(new NamedPropertyParameter(nameof(IPrototype.IsAbstract), isAbstract));
        }
        if (basePrototype.HasValue)
        {
            parameters = parameters.Append(new NamedPropertyParameter(nameof(IPrototype.Base), basePrototype));
        }

        return container.Resolve<IPrototype<TSubject>>(parameters);
    }
}
