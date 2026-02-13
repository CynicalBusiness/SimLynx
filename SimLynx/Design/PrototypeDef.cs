
using System;
using Autofac;
using SimLynx.Core;

namespace SimLynx.Design;

/// <summary>
/// Definition for creating a prototype.
/// </summary>
public class PrototypeDef(Symbol name) : IHaveSymbolicName
{

    /// <summary>
    /// The name of the prototype, which is used for registration.
    /// </summary>
    public Symbol Name { get; } = name;

    /// <summary>
    /// The def this def extends from, if any.
    /// </summary>
    public PrototypeDef? Extends { get; init; }

    /// <summary>
    /// Creates a new prototype based on this definition in the provided scope.
    /// </summary>
    /// <param name="scope">The scope in which to create the prototype.</param>
    /// <returns>A new instance of the prototype.</returns>
    public Prototype Create(ILifetimeScope scope)
    {
        // TODO remember to check for cycles!
        throw new NotImplementedException();
    }

}
