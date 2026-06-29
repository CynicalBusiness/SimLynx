using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimLynx.Core.Prototyping.Properties;

/// <summary>
/// Prototype config for a class property of a prototype's subject type.
/// </summary>
public interface IPrototypePropertyConfig : IPrototypeConfig
{
    /// <summary>
    /// The property information for the target property of this configuration.
    /// </summary>
    public PropertyInfo Property { get; }

    /// <summary>
    /// The current value of this configuration, if any.
    /// </summary>
    public Maybe<object?> Value { get; set; }

    /// <summary>
    /// Whether this configuration has any modifiers defined.
    /// </summary>
    public bool HasModifiers { get; }

    /// <summary>
    /// The modifiers for this configuration, which are applied to the value when it is set. Modifiers are applied in
    /// the order they are defined.
    /// </summary>
    public IEnumerable<Func<object?, object?>> Modifiers { get; }

    /// <summary>
    /// Adds a modifier to this configuration, which will be applied to the value when it is set. Modifiers are applied
    /// in the order they are defined.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    public void ModifyValue(Func<object?, object?> modifier);

    /// <summary>
    /// Indicates whether this configuration has any value or modifiers defined, and is therefore considered "configured".
    /// </summary>
    public bool IsConfigured => Value.HasValue || HasModifiers;
}
