using System;
using System.ComponentModel.DataAnnotations;
using SimLynx.Core.Prototyping;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Base class for a prototype of a <see cref="Component"/>.
/// </summary>
public class ComponentPrototype : Prototype
{
    /// <inheritdoc cref="ComponentPrototype"/>
    public ComponentPrototype()
        : base()
    {
        ComponentType = DefaultComponentType;
    }

    /// <summary>
    /// The type of component represented by this prototype.
    /// </summary>
    [Required]
    public virtual IComponentType ComponentType
    {
        get => field;
        set
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            if (field is not null && !value.Type.IsAssignableTo(field.Type))
            {
                throw new ArgumentException(
                    $"Component type '{value.Type.FullName}' is not compatible with existing component type '{field.Type.FullName}'",
                    nameof(value)
                );
            }

            field = value;
        }
    }

    /// <summary>
    /// Whether this prototype represents an abstract component type (and thus cannot be used to create components).
    /// </summary>
    public bool IsAbstract => ComponentType.IsAbstract;

    /// <summary>
    /// The default component type for this prototype class.
    /// </summary>
    protected virtual IComponentType DefaultComponentType => ComponentTypes.For<Component>();
}
