
using SimLynx.Core.Defs;

namespace SimLynx.Design.Prototypes;

/// <summary>
/// Generalized interface for a <see cref="Prototype"/>.
/// </summary>
/// <remarks>
/// Contains the bare minimum for a prototype.
/// </remarks>
public interface IPrototype : IDefObject
{
    /// <summary>
    /// Identifier for this prototype.
    /// </summary>
    public Symbol Id { get; }
}
