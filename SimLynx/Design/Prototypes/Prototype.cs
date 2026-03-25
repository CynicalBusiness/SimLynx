
using SimLynx.Core.Defs;

namespace SimLynx.Design.Prototypes;

/// <summary>
/// The standard base class for prototypes.
/// </summary>
/// <remarks>
/// Prototypes are the core building blocks of SimLynx's design system. They represent reusable templates that can be
/// instantiated and customized to create various entities within the system. They can also function similar to
/// ECS-style systems for their relevant entities.
/// </remarks>
public class Prototype : IPrototype, IDefObject<PrototypeDef>
{

    /// <inheritdoc/>
    public required Symbol Id { get; init; }

    /// <inheritdoc/>
    public required PrototypeDef Def { get; init; }

}
