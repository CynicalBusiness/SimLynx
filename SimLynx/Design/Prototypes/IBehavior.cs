
using SimLynx.Core.Defs;

namespace SimLynx.Design.Prototypes;

/// <summary>
/// Generalized interface for a prototype <see cref="Behavior"/>.
/// </summary>
/// <remarks>
/// Contains the bare minimum for a prototype behavior.
/// </remarks>
public interface IBehavior : IDefObject
{

    /// <summary>
    /// The prototype to which this behavior is attached.
    /// </summary>
    public IPrototype Prototype { get; }

}
