using System;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Static contextual information about a component in an entity tree.
/// </summary>
public class ComponentInfo : IComponentAddressable
{
    internal ComponentInfo(ComponentAddress address, Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType, nameof(componentType));
        if (!componentType.IsClass || componentType.IsAbstract || !componentType.IsAssignableTo(typeof(Component)))
            throw new ArgumentException(
                $"The type {componentType} is not a valid concrete component class type.",
                nameof(componentType)
            );

        Address = address;
        ComponentType = componentType;
    }

    /// <summary>
    /// The concrete component class type this info describes.
    /// </summary>
    public Type ComponentType { get; }

    /// <summary>
    /// The name of the component.
    /// </summary>
    /// <remarks>
    /// Not required to be unique among its siblings.
    /// </remarks>
    public Symbol Name { get; init; }

    /// <inheritdoc/>
    public ComponentAddress Address { get; }
}
