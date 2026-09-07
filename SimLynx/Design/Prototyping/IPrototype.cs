using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SimLynx.Design.Prototyping.Blueprints;

namespace SimLynx.Design.Prototyping;

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
    /// The name of this prototype.
    /// </summary>
    public Identifier Name { get; }

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
    /// Gets an enumeration of all slots configured directly on this prototype.
    /// </summary>
    public IEnumerable<IPrototypeConfigSlot> Slots { get; }

    /// <summary>
    /// Gets a config slot on this prototype by its <paramref name="slotId"/>, if possible.
    /// </summary>
    /// <param name="slotId">The ID of the slot to retrieve.</param>
    /// <returns>The config slot if found; otherwise, null.</returns>
    public IPrototypeConfigSlot? this[Identifier slotId] { get; }

    /// <summary>
    /// Tries to resolve a config slot on this prototype by its <paramref name="slotId"/>.
    /// </summary>
    /// <param name="slotId">The ID of the slot to resolve.</param>
    /// <param name="slot">The resolved slot, if found.</param>
    /// <returns>True if the slot was found; otherwise, false.</returns>
    public bool TryGetSlot(Identifier slotId, [MaybeNullWhen(false)] out IPrototypeConfigSlot slot);

    /// <summary>
    /// Tries to resolve a config slot on this prototype which supports the <typeparamref name="TConfig"/> type.
    /// </summary>
    /// <typeparam name="TConfig">The type of configuration the slot must support.</typeparam>
    /// <param name="slot">The resolved slot, if found.</param>
    /// <returns>True if the slot was found; otherwise, false.</returns>
    public bool TryGetSlot<TConfig>([MaybeNullWhen(false)] out IPrototypeConfigSlotOf<TConfig> slot)
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
