using System.Reflection;

namespace SimLynx.Design.Prototyping.Properties;

/// <summary>
/// Prototype config for a class property.
/// </summary>
public interface IPropertyConfig : IPrototypeValueConfig
{
    /// <summary>
    /// The property information for the target property of this configuration.
    /// </summary>
    public PropertyInfo Property { get; }
}

/// <summary>
/// Prototype config for a class property of a <typeparamref name="TSubject"/> subject type.
/// </summary>
public interface IPropertyConfig<TSubject> : IPropertyConfig, IPrototypeConfig<TSubject>
    where TSubject : class, IPrototypeSubject { }

/// <summary>
/// Prototype config for a <typeparamref name="TValue"/> class property of a <typeparamref name="TSubject"/> subject
/// type.
/// </summary>
/// <typeparam name="TSubject">The type of the subject this configures.</typeparam>
/// <typeparam name="TValue">The type of value this property configures.</typeparam>
public interface IPropertyConfig<TSubject, TValue> : IPropertyConfig<TSubject>, IPrototypeValueConfig<TSubject, TValue>
    where TSubject : class, IPrototypeSubject { }
