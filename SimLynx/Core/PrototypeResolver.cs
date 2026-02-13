
using Autofac.Features.Indexed;

namespace SimLynx.Core;

/// <summary>
/// Resolver for prototypes, allowing retrieval of prototypes by name.
/// </summary>
/// <param name="prototypeIndex">The index of prototypes.</param>
public class PrototypeResolver(IIndex<Symbol, Prototype> prototypeIndex)
{
    /// <summary>
    /// Gets the prototype with the given name.
    /// </summary>
    /// <param name="name">The name of the prototype.</param>
    /// <returns>The prototype with the given name.</returns>
    public Prototype this[Symbol name] => prototypeIndex[name];

    /// <summary>
    /// Tries to get the prototype with the given name.
    /// </summary>
    /// <param name="name">The name of the prototype.</param>
    /// <param name="prototype">The prototype if found; otherwise, null.</param>
    /// <returns>True if the prototype was found; otherwise, false.</returns>
    public bool TryGetPrototype(Symbol name, out Prototype? prototype)
    {
        return prototypeIndex.TryGetValue(name, out prototype);
    }
}
