using System.Numerics;
using SimLynx.Simulation.ComponentModel;

namespace SimLynx.Features.World;

/// <summary>
/// Component which represents a physical 3D transformation (position, rotation, scale) of an entity in the world.
/// </summary>
public class Transform(IComponentBuildContext ctx) : Component(ctx)
{
    /// <summary>
    /// State for the local transformation matrix.
    /// </summary>
    protected readonly IInstanceState<Matrix4x4> transform = ctx.RegisterState<Matrix4x4>();
}
