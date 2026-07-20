using System;

namespace SimLynx.Core.Prototyping.Properties;

/// <summary>
/// Allows for flagging a prototype's property as configurable (or not).
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
    public ConfigurableAttribute()
        : this(true) { }

    /// <summary>
    /// Indicates whether the property is configurable by its corresponding prototype. If false, the property will be ignored
    /// when configuring a prototype object from a prototype.
    /// </summary>
    public bool IsConfigurable { get; } = isConfigurable;
}
