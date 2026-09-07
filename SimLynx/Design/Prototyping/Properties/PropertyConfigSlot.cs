using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using SimLynx.Design.Prototyping.Blueprints;

namespace SimLynx.Design.Prototyping.Properties;

/// <summary>
/// Static helpers for working with prototype property configs.
/// </summary>
public static class PropertyConfigSlot
{
    /// <summary>
    /// The preferred slot ID for property configs.
    /// </summary>
    public static Identifier SlotId { get; } = "properties";

    /// <summary>
    /// Type key used to store cached configurable properties for subject types.
    /// </summary>
    public static TypeKey<IReadOnlyDictionary<string, PropertyInfo>> ConfigurablePropertiesKey { get; } =
        new("configurableProperties");
}

/// <summary>
/// Config slot for property configs, which configure properties of a subject type via reflection.
/// </summary>
/// <inheritdoc cref="PrototypeConfigSlot{TSubject, TConfig}"/>
public class PropertyConfigSlot<TSubject>(IPrototype<TSubject> prototype, IMetaType<TSubject> subjectMetaType)
    : PrototypeConfigSlot<TSubject, IPropertyConfig<TSubject>>(prototype)
    where TSubject : class, IPrototypeSubject
{
    /// <summary>
    /// Attempts to extract a property name from the given <paramref name="identifier"/>. Returns true only if the identifier
    /// is unqualified and its specifier is a named symbol.
    /// </summary>
    /// <param name="identifier">The identifier to extract the property name from.</param>
    /// <param name="propertyName">The extracted property name, if successful.</param>
    /// <returns>True if the property name was successfully extracted; otherwise, false.</returns>
    public static bool TryExtractPropertyName(Identifier identifier, [NotNullWhen(true)] out string? propertyName)
    {
        if (identifier.IsQualified || !identifier.Specifier.IsNamed)
        {
            // only unqualified named identifiers will map
            propertyName = null;
            return false;
        }

        propertyName = identifier.Specifier.Description;
        return true;
    }

    /// <summary>
    /// Gets an identifier for a property with the given <paramref name="propertyName"/>.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The identifier for the property.</returns>
    public static Identifier GetPropertyIdentifier(string propertyName)
    {
        return new(propertyName);
    }

    /// <inheritdoc/>
    public override void Configure(IBlueprintBuilder<TSubject> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        var effectiveConfigs = GetEffectiveConfigs(builder);

        var unsetRequiredProperties = effectiveConfigs
            .Values.Where(config => config.Property.IsRequired && !config.HasValue)
            .ToArray();

        if (unsetRequiredProperties.Length > 0)
        {
            throw new PrototypeCompilationException(
                Prototype,
                $"Required properties are not set: {string.Join(", ", unsetRequiredProperties.Select(c => c.Name))}"
            );
        }

        base.Configure(builder);
    }

    /// <inheritdoc/>
    protected override IPropertyConfig<TSubject>? Create(Identifier name)
    {
        if (!TryExtractPropertyName(name, out var propertyName) || !TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return PropertyConfig.Create<TSubject>(property);
    }

    private bool TryGetProperty(string? propertyName, [MaybeNullWhen(false)] out PropertyInfo property)
    {
        if (propertyName is null)
        {
            property = null;
            return false;
        }

        var props = subjectMetaType.Metadata.GetOrAdd(
            PropertyConfigSlot.ConfigurablePropertiesKey,
            () =>
                subjectMetaType
                    .Type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.IsPrototypeConfigurable)
                    .ToImmutableDictionary(p => p.Name)
        );

        return props.TryGetValue(propertyName, out property);
    }
}
