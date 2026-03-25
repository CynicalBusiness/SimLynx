
using SimLynx.Core.Defs;

namespace SimLynx.Design.Prototypes;

/// <summary>
/// The standard base class for prototype behaviors.
/// </summary>
/// <remarks>
/// Behaviors allow for designing reusable component-like logic that can be attached to prototypes.
/// </remarks>
public class Behavior : IBehavior, IDefObject<BehaviorDef>
{

    /// <inheritdoc cref="IBehavior.Prototype"/>
    public required Prototype Prototype { get; init; }

    /// <inheritdoc />
    public required BehaviorDef Def { get; init; }

    IPrototype IBehavior.Prototype => Prototype;

}
