using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac;
using Autofac.Features.Indexed;
using SimLynx.Core.Prototyping.Blueprints;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// Standard base implementation of a prototype.
/// </summary>
/// <remarks>
/// Contains default implementations and helpers suitable for most prototypes.
/// </remarks>
public abstract class Prototype<TSubject>(
    Symbol id,
    IIndex<Symbol, IEnumerable<IPrototypeConfigResolver>> resolversIndex
) : IPrototype<TSubject>
    where TSubject : class, IPrototypeSubject
{
    private readonly Dictionary<Symbol, Dictionary<string, IPrototypeConfig>> configs = [];

    /// <inheritdoc/>
    public Symbol Id { get; } = id;

    /// <inheritdoc cref="IPrototype.Base"/>
    public virtual IPrototype? Base
    {
        get => field;
        init
        {
            if (value is not null && !value.SubjectType.IsAssignableFrom(SubjectType))
            {
                throw new ArgumentException(
                    $"Base prototype '{value.Id}' is of type {value.SubjectType}, which is not a base type of this prototype's subject type: {SubjectType}.",
                    nameof(value)
                );
            }

            field = value;
        }
    }

    /// <inheritdoc/>
    public virtual bool IsAbstract { get; init; } = false;

    /// <inheritdoc/>
    public virtual Type SubjectType => typeof(TSubject);

    /// <summary>
    /// Gets the currently-defined config, if any, for the given <paramref name="slot"/> and <paramref name="configName"/>.
    /// </summary>
    /// <param name="slot">The slot for which to retrieve the config.</param>
    /// <param name="configName">The name of the config to retrieve.</param>
    /// <returns>The config if found; otherwise, <c>null</c>.</returns>
    public IPrototypeConfig? this[Symbol slot, string configName]
    {
        get
        {
            if (configs.TryGetValue(slot, out var configsForSlot))
            {
                if (configsForSlot.TryGetValue(configName, out var config))
                {
                    return config;
                }
            }
            return default;
        }
        private set
        {
            configs.TryGetValue(slot, out var configsForSlot);

            if (value is null)
            {
                if (configsForSlot is not null)
                {
                    configsForSlot.Remove(configName);
                    if (configsForSlot.Count == 0)
                    {
                        configs.Remove(slot);
                    }
                }
            }
            else
            {
                if (configsForSlot is null)
                {
                    configsForSlot = [];
                    configs[slot] = configsForSlot;
                }
                configsForSlot[configName] = value;
            }
        }
    }

    /// <inheritdoc/>
    public IBlueprint<TSubject> Compile()
    {
        throw new NotImplementedException(); // TODO
    }

    /// <inheritdoc/>
    public bool TryGetConfig<TConfig>(Symbol slot, string configName, [MaybeNullWhen(false)] out TConfig config)
        where TConfig : class, IPrototypeConfig
    {
        // try to find an existing config for this slot and name
        var currentConfig = this[slot, configName];
        if (currentConfig is not null)
        {
            if (currentConfig is not TConfig typedConfig)
            {
                throw new InvalidOperationException(
                    $"An existing config for '{slot}/{configName}' is of type {currentConfig.GetType().FullName}, which is not assignable to the requested type {typeof(TConfig).FullName}."
                );
            }
            config = typedConfig;
            return true;
        }

        // none found, try to create one from a provider
        foreach (var resolver in resolversIndex[slot])
        {
            if (resolver is IPrototypeConfigProvider<TConfig> provider)
            {
                config = provider.TryCreate(this, configName);
                if (config is not null)
                {
                    this[slot, configName] = config;
                    return true;
                }
            }
        }

        config = default;
        return false;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{GetType().Name}<{typeof(TSubject).Name}>#{Id}";
    }
}
