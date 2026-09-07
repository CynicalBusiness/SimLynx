using System;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// An address that describes a particular component in an entity tree.
/// </summary>
/// <remarks>
/// The exact structure of a component address is intentionally opaque, and should not be relied upon. It is only
/// guaranteed to be unique and stable within a single entity tree.
/// </remarks>
public readonly struct ComponentAddress
{
    /// <summary>
    /// An invalid component address which does not describe any component in any entity tree.
    /// </summary>
    public static readonly ComponentAddress Invalid = default;

    // keep opaque

    /// <summary>
    /// Indicates whether this address is valid.
    /// </summary>
    /// <remarks>
    /// Does not indicate whether it describes a resolvable component, only that it is valid structurally.
    /// </remarks>
    public bool IsValid => throw new NotImplementedException();
}
