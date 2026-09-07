using System.Numerics;
using SimLynx.Simulation.ComponentModel;

namespace SimLynx.Features.World;

/// <summary>
/// Component which moves an entity in the world.
/// </summary>
public class Mover(IComponentContextInfo ctx) : Component(ctx)
{
    /// <summary>
    /// The transform this mover will move.
    /// </summary>
    protected readonly ComponentRef<Transform> transform;

    /// <summary>
    /// State for the current local velocity, in units per second, applied to the position of the transform.
    /// </summary>
    protected readonly InstanceState<Vector3> velocity;

    /// <summary>
    /// State for the current local acceleration, in units per second squared, applied to the velocity of the transform
    /// before it is applied to the position.
    /// </summary>
    protected readonly InstanceState<Vector3> acceleration;
}
