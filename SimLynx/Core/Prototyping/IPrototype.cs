using System;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// General interface for prototypes.
/// </summary>
public interface IPrototype
{
    /// <summary>
    /// The ID of this prototype.
    /// </summary>
    public Symbol Id { get; }
}
