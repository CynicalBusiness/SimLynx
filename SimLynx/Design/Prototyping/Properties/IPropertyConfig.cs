using System;
using System.Reflection;

namespace SimLynx.Design.Prototyping.Properties;

/// <summary>
/// Prototype config for a class property.
/// </summary>
public interface IPropertyConfig : IPrototypeConfig
{
    /// <summary>
    /// The property information for the target property of this configuration.
    /// </summary>
    public PropertyInfo Property { get; }

    /// <summary>
    /// The type of the value for this configuration.
    /// </summary>
    public Type ValueType => Property.PropertyType;

    /// <summary>
    /// Whether this configuration has a value to apply to the target property.
    /// </summary>
    /// <remarks>
    /// A configuration for a required property which has no value may be an error if the type is unable to be constructed.
    /// </remarks>
    public bool HasValue { get; }

    /// <summary>
    /// Whether this configuration has any configurations to apply to the target property.
    /// </summary>
    public bool HasConfigurations { get; }

    /// <summary>
    /// Indicates whether this config sets a value for its respective property.
    /// </summary>
    /// <remarks>
    /// May differ from <see cref="HasValue"/> if the property is able to set a default.
    /// </remarks>
    public bool CanSetValue { get; }
}

/// <summary>
/// Prototype config for a class property of a prototype's subject type.
/// </summary>
public interface IPropertyConfig<TSubject> : IPropertyConfig, IPrototypeConfig<TSubject>
    where TSubject : class, IPrototypeSubject
{
    /// <summary>
    /// Copies the value and configurations of this config to the provided <paramref name="other"/> config.
    /// </summary>
    /// <param name="other">The other config to copy to.</param>
    public void CopyTo(IPropertyConfig<TSubject> other);

    /// <summary>
    /// Configures the property with the provided <paramref name="configurationFunc"/>. The function is invoked with
    /// the current value and should return a new or modified value to set for the property.
    /// </summary>
    /// <param name="configurationFunc">The configuration function to apply to the property.</param>
    public void Configure(PropertyConfig<TSubject, object?>.ConfigurationFunc configurationFunc);

    /// <summary>
    /// Configures the property with the provided <paramref name="valueFunc"/>. The function is invoked to provide a
    /// new value to set for the property, overriding any previous value or configuration for the property.
    /// </summary>
    /// <param name="valueFunc">The function to provide the new value for the property.</param>
    public void Configure(PropertyConfig<TSubject, object?>.ValueFunc valueFunc);

    /// <summary>
    /// Configures the property with the provided <paramref name="action"/>. The action is invoked with the current
    /// value and may modify it in place. Should only be used for mutable property types.
    /// </summary>
    /// <param name="action">The modification action to apply to the property.</param>
    public void Configure(Action<object?> action);
}
