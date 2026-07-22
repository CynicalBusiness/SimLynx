using System.Collections.Generic;
using System.Linq;
using SimLynx.Core.Prototyping.Blueprints;

namespace SimLynx.Core.Prototyping.Properties;

/// <summary>
/// Static helpers for working with prototype property configs.
/// </summary>
public static class PropertyConfigSlot
{
    /// <summary>
    /// The preferred slot ID for property configs.
    /// </summary>
    public static readonly Symbol SLOT_ID = "Properties";
}

/// <summary>
/// Config slot for property configs, which configure properties of a subject type via reflection.
/// </summary>
/// <inheritdoc cref="PrototypeConfigSlot{TSubject, TConfig}"/>
public class PropertyConfigSlot<TSubject>(IPrototype<TSubject> prototype, Symbol slotId)
    : PrototypeConfigSlot<TSubject, IPropertyConfig<TSubject>>(prototype, slotId)
    where TSubject : class, IPrototypeSubject
{
    /// <inheritdoc/>
    public override void Configure(IBlueprintBuilder<TSubject> builder)
    {
        var effectiveConfigs = GetEffectiveConfigs(builder);

        foreach (var config in effectiveConfigs.Values)
        {
            config.Apply(builder);
        }
    }

    /// <inheritdoc/>
    protected override IPropertyConfig<TSubject>? Create(string name)
    {
        var typeInfo = PrototypePropertyTypeInfo.For<TSubject>();

        if (!typeInfo.TryGetProperty(name, out var property))
        {
            return null;
        }

        return PropertyConfig.Create<TSubject>(property);
    }

    /// <summary>
    /// Gets a dictionary of effective property configs for this slot, walking the prototype hierarchy as necessary.
    /// </summary>
    /// <remarks>
    /// The configs are stabilized, meaning if the configs change in the relevant prototype, this dictionary will not
    /// reflect those changes and can be used to configure a blueprint without being affected by changes in the prototype
    /// hierarchy.
    /// <br/>
    /// Note: The effective configs are computed once per builder and cached. Calling this method again with the same
    /// builder will return a reference to the same dictionary.
    /// </remarks>
    /// <returns>A dictionary of effective property configs, keyed by property name.</returns>
    protected virtual Dictionary<string, IPropertyConfig<TSubject>> GetEffectiveConfigs(
        IBlueprintBuilder<TSubject> builder
    )
    {
        builder.Options.GetOrAdd<Dictionary<string, IPropertyConfig<TSubject>>>(out var effectiveConfigs);

        var allConfigs = Prototype
            .GetAncestorsFromRoot(includeSelf: true)
            .SelectMany(prototype => prototype[Id]?.GetOwn() ?? [])
            .OfType<IPropertyConfig>();

        foreach (var config in allConfigs)
        {
            if (!effectiveConfigs.TryGetValue(config.Name, out var effectiveConfig))
            {
                // var typeInfo = PrototypePropertyTypeInfo.For<TSubject>();
                // if (!typeInfo.TryGetProperty(config.Name, out var property))
                // {
                //     continue;
                // }

                effectiveConfigs[config.Name] = effectiveConfig = PropertyConfig.Create<TSubject>(config.Property);
            }

            if (config is IPropertyConfigState configState)
            {
                configState.CopyTo(effectiveConfig);
            }
        }

        return effectiveConfigs;
    }
}
