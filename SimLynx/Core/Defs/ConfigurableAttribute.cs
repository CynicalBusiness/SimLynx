
using System;

namespace SimLynx.Core.Defs;

/// <summary>
/// Allows for flagging a def object's property as (not) configurable by its corresponding def, as well as controlling
/// how this property is configured.
/// </summary>
/// <remarks>
/// This attribute can be used to override the default rules for when and how a property is configured, such as allowing
/// a non-public property to be configured (as it otherwise would not be) or preventing a public property from being
/// configured.
/// </remarks>
/// <param name="isConfigurable">Sets whether the property <see cref="IsConfigurable"/>.</param>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class ConfigurableAttribute(bool isConfigurable) : Attribute
{

    /// <summary>
    /// Marks this property as configurable, even if it would otherwise not be (e.g. if it's non-public).
    /// </summary>
    public ConfigurableAttribute() : this(true) { }

    /// <summary>
    /// Indicates whether the property is configurable by its corresponding def. If false, the property will be ignored
    /// when configuring a def object from a def.
    /// </summary>
    public bool IsConfigurable { get; } = isConfigurable;

}
