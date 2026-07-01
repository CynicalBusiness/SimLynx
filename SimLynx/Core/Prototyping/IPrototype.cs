using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SimLynx.Core.Prototyping.Blueprints;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// Base interface for all prototypes.
/// </summary>
/// <remarks>
/// In essence, a prototype is a object that contains information necessary to create, configure, or otherwise build
/// an object in which it is a prototype of.
/// <br/>
/// Prototypes can be thought of as similar to "runtime classes" or "JiT classes" common in dynamic/prototypical
/// languages (eg. JavaScript); prototypes have many parallels to "AoT" class types in languages like C#, but are
/// designed at runtime.
/// </remarks>
public interface IPrototype
{
    /// <summary>
    /// The ID of this prototype.
    /// </summary>
    public Symbol Id { get; }

    /// <summary>
    /// The base prototype of this prototype, if any.
    /// </summary>
    /// <remarks>
    /// This base prototype is used to implement prototype inheritance, where a prototype can inherit properties and
    /// configuration from another prototype <em>and</em> allows for "instanceof"-like checks for objects against
    /// prototypes.
    /// </remarks>
    public IPrototype? Base { get; }

    /// <summary>
    /// Indicates this prototype is abstract, meaning it cannot be compiled into a blueprint directly, but can be used
    /// as a base prototype for other prototypes.
    /// </summary>
    public bool IsAbstract { get; }

    /// <summary>
    /// The type of object this prototype is a prototype of.
    /// </summary>
    public Type SubjectType { get; }

    /// <summary>
    /// Tries to get a config for the slot with the given <paramref name="slot"/> on this prototype at the given
    /// <paramref name="configName"/>.
    /// </summary>
    /// <param name="slot">The ID of the slot.</param>
    /// <param name="configName">The name of the config.</param>
    /// <param name="config">The fetched/created configuration, if found/valid for this prototype.</param>
    /// <returns>True if the configuration was found; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">If no such slot exists for the given <paramref name="slot"/>.</exception>
    public bool TryGetConfig(Symbol slot, string configName, [MaybeNullWhen(false)] out IPrototypeConfig config)
    {
        return TryGetConfig<IPrototypeConfig>(slot, configName, out config);
    }

    /// <summary>
    /// Tries to get a config for the slot with the given <paramref name="slot"/> on this prototype at the given
    /// <paramref name="configName"/>.
    /// </summary>
    /// <typeparam name="TConfig">The type of configuration to fetch or create.</typeparam>
    /// <param name="slot">The ID of the slot.</param>
    /// <param name="configName">The name of the config.</param>
    /// <param name="config">The fetched/created configuration, if found/valid for this prototype.</param>
    /// <returns>True if the configuration was found; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">If no such slot exists for the given <paramref name="slot"/>.</exception>
    public bool TryGetConfig<TConfig>(Symbol slot, string configName, [MaybeNullWhen(false)] out TConfig config)
        where TConfig : class, IPrototypeConfig;

    /// <summary>
    /// Compiles this prototype into an immutable blueprint, which can be used as a factory to create subject instances.
    /// </summary>
    /// <returns>A compiled blueprint for this prototype.</returns>
    public IBlueprint Compile();
}

/// <inheritdoc cref="IPrototype"/>
/// <typeparam name="TSubject">The type of object this prototype prototypes.</typeparam>
public interface IPrototype<out TSubject> : IPrototype
    where TSubject : class, IPrototypeSubject
{
    /// <inheritdoc cref="IPrototype.Compile"/>
    public new IBlueprint<TSubject> Compile();

    IBlueprint IPrototype.Compile() => Compile();
}
