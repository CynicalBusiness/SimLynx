using System;
using Autofac;

namespace SimLynx.Core.Prototyping.Properties;

/// <summary>
/// Prototype config provider for <see cref="IPrototypePropertyConfig"/>.
/// </summary>
public class PrototypePropertyConfigProvider : IPrototypeConfigProvider<IPrototypePropertyConfig>
{
    /// <summary>
    /// The slot for prototype property configs.
    /// </summary>
    public const string PROPERTIES_SLOT_NAME = "Properties";

    /// <inheritdoc/>
    public required Symbol Slot { get; init; }

    /// <inheritdoc/>
    public IPrototypePropertyConfig? TryCreate<TSubject>(IPrototype<TSubject> prototype, string name)
        where TSubject : class, IPrototypeSubject
    {
        var typeInfo = PrototypePropertyTypeInfo.For<TSubject>();

        if (!typeInfo.TryGetProperty(name, out var property))
        {
            return null;
        }

        return (IPrototypePropertyConfig)
            Activator.CreateInstance(
                typeof(PrototypePropertyConfig<>).MakeGenericType(property.PropertyType),
                this,
                property
            )!;
    }
}
