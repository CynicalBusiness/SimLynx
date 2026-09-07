using System;
using System.Collections.Generic;
using Autofac;
using SimLynx.Design.Prototyping.Blueprints;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Base implementation of a prototype config slot.
/// </summary>
/// <typeparam name="TSubject">The concrete subject type being configured.</typeparam>
/// <typeparam name="TConfig">The type of configuration this slot accepts.</typeparam>
/// <param name="prototype">The prototype that owns this config slot.</param>
public abstract class PrototypeConfigSlot<TSubject, TConfig>(IPrototype<TSubject> prototype)
    : IPrototypeConfigSlot<TSubject, TConfig>
    where TSubject : class, IPrototypeSubject
    where TConfig : class, IPrototypeConfig<TSubject>
{
    /// <summary>
    /// The prototype that owns this config slot.
    /// </summary>
    protected IPrototype<TSubject> Prototype { get; } = prototype;

    /// <summary>
    /// Dictionary of configurations in this slot, keyed by their name.
    /// </summary>
    protected Dictionary<Identifier, TConfig> Configs { get; } = [];

    /// <inheritdoc/>
    [ServiceKey]
    public required Identifier Id { get; init; }

    /// <inheritdoc/>
    public virtual TConfig? this[Identifier id]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            if (Configs.TryGetValue(id, out var config))
            {
                return config;
            }

            if ((config = Create(id)) is not null)
            {
                Configs[id] = config;
                return config;
            }

            return null;
        }
    }

    /// <inheritdoc/>
    public virtual bool IsSupported => true;

    /// <summary>
    /// Type key used to cache effective configs.
    /// </summary>
    protected virtual TypeKey<Dictionary<Identifier, TConfig>> EffectiveConfigsCacheKey => new(Id);

    /// <inheritdoc/>
    public virtual IEnumerable<TConfig> GetOwn()
    {
        return Configs.Values;
    }

    /// <summary>
    /// Configures this slot for the given <paramref name="builder"/>. This is called during prototype compilation to allow
    /// the slot to configure the blueprint with any necessary information.
    /// </summary>
    /// <param name="builder">The blueprint builder to configure.</param>
    public virtual void Configure(IBlueprintBuilder<TSubject> builder)
    {
        var effectiveConfigs = GetEffectiveConfigs(builder);
        foreach (var config in effectiveConfigs.Values)
        {
            config.Apply(builder);
        }
    }

    /// <summary>
    /// Gets all configurations in this slot, including those inherited from ancestor prototypes, in order from the
    /// root prototype to the most derived.
    /// </summary>
    /// <returns>The enumeration of all configs</returns>
    protected virtual IEnumerable<TConfig> GetAll()
    {
        foreach (var ancestor in Prototype.GetAncestorsFromRoot(includeSelf: true))
        {
            if (ancestor[Id] is IPrototypeConfigSlotOf<TConfig> slot)
            {
                foreach (var config in slot.GetOwn())
                {
                    yield return config;
                }
            }
        }
    }

    /// <summary>
    /// Builds a dictionary of effective configs for this slot, walking the prototype hierarchy top-down and copying
    /// each config into a new "effective" config for each name.
    /// </summary>
    /// <remarks>
    /// This method will attempt to cache the results on the given <paramref name="builder"/>, unless it is <c>null</c>.
    /// Repeat calls with the same non-null builder will return a reference to the same dictionary without rebuilding,
    /// even if the prototype changed in the meantime.
    /// </remarks>
    /// <param name="builder">A blueprint builder to use for caching the effective configs, or <c>null</c> to skip caching.</param>
    /// <returns>A dictionary of effective configs keyed by their identifier.</returns>
    protected virtual Dictionary<Identifier, TConfig> GetEffectiveConfigs(IBlueprintBuilder<TSubject>? builder)
    {
        var cacheKey = EffectiveConfigsCacheKey;

        if (builder is not null && builder.Options.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var configs = new Dictionary<Identifier, TConfig>(Identifier.Comparer.Default);

        foreach (var config in GetAll())
        {
            if (!configs.TryGetValue(config.Name, out var effectiveConfig))
            {
                effectiveConfig = Create(config.Name);
                if (effectiveConfig is null)
                {
                    break;
                }

                configs[config.Name] = effectiveConfig;
            }

            config.CopyTo(effectiveConfig);
        }

        builder?.Options.Set(cacheKey, configs);
        return configs;
    }

    /// <summary>
    /// Attempts to create a configuration for the given <paramref name="id"/> in this slot. If it cannot be created,
    /// may return <c>null</c> to indicate configuration is not supported, but is not an error.
    /// </summary>
    /// <param name="id">The identifier of the configuration to create.</param>
    /// <returns>The created configuration, or <c>null</c> if it cannot be created.</returns>
    protected abstract TConfig? Create(Identifier id);
}
