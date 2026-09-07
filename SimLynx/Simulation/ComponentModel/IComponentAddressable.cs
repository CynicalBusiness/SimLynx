namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Generic interface for objects which have a <see cref="ComponentAddress"/>.
/// </summary>
public interface IComponentAddressable
{
    /// <summary>
    /// The address of the component in its tree.
    /// </summary>
    public ComponentAddress Address { get; }
}
