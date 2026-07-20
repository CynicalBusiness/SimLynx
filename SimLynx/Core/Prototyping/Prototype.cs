using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SimLynx.Core.Prototyping.Blueprints;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// Standard base implementation of a prototype.
/// </summary>
/// <remarks>
/// Contains default implementations and helpers suitable for most prototypes.
/// </remarks>
public class Prototype<TSubject>(Symbol id, PrototypeContext<TSubject> context) : IPrototype<TSubject>
    where TSubject : class, IPrototypeSubject
{
    private readonly Dictionary<Type, IPrototypeConfigSlot<TSubject, IPrototypeConfig>?> typedSlotsCache = [];

    /// <summary>
    /// Dictionary of config slots for this prototype, keyed by their slot ID.
    /// </summary>
    protected Dictionary<Symbol, IPrototypeConfigSlot<TSubject, IPrototypeConfig>> Slots { get; } = [];

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
    /// Gets the currently-defined config, if any, for the given <paramref name="slotId"/> and <paramref name="configName"/>.
    /// </summary>
    /// <param name="slotId">The slot for which to retrieve the config.</param>
    /// <param name="configName">The name of the config to retrieve.</param>
    /// <returns>The config if found; otherwise, <c>null</c>.</returns>
    public IPrototypeConfig? this[Symbol slotId, string configName] => this[slotId]?[configName];

    /// <inheritdoc cref="IPrototype.this[Symbol]"/>
    public IPrototypeConfigSlot<TSubject, IPrototypeConfig>? this[Symbol slotId] => Slots.GetValueOrDefault(slotId);

    IPrototypeConfigSlot? IPrototype.this[Symbol slotId] => this[slotId];

    /// <summary>
    /// Resolver for config slots for this prototype.
    /// </summary>
    protected PrototypeConfigSlotResolver<TSubject> ConfigSlotResolver => context.ConfigSlotResolverFactory(this);

    /// <inheritdoc/>
    public IBlueprint<TSubject> Compile()
    {
        var builder = new BlueprintBuilder<TSubject>(this);

        foreach (var slot in Slots.Values)
        {
            slot.Configure(builder);
        }

        return builder.Build();
    }

    /// <inheritdoc cref="IPrototype.TryGetSlot"/>
    public bool TryGetSlot(
        Symbol slotId,
        [MaybeNullWhen(false)] out IPrototypeConfigSlot<TSubject, IPrototypeConfig> slot
    )
    {
        if (!Slots.TryGetValue(slotId, out slot))
        {
            if (
                ConfigSlotResolver.TryResolve(slotId, out var resolvedSlot)
                && resolvedSlot is IPrototypeConfigSlot<TSubject, IPrototypeConfig> typedSlot
            )
            {
                Slots[slotId] = typedSlot;
                slot = typedSlot;
                return true;
            }
            else
            {
                slot = default;
                return false;
            }
        }

        return true;
    }

    bool IPrototype.TryGetSlot(Symbol slotId, [MaybeNullWhen(false)] out IPrototypeConfigSlot slot)
    {
        if (TryGetSlot(slotId, out var typedSlot))
        {
            slot = typedSlot;
            return true;
        }

        slot = default;
        return false;
    }

    /// <inheritdoc cref="IPrototype.TryGetSlot{TConfig}"/>
    public bool TryGetSlot<TConfig>([MaybeNullWhen(false)] out IPrototypeConfigSlot<TSubject, TConfig> slot)
        where TConfig : class, IPrototypeConfig
    {
        if (typedSlotsCache.TryGetValue(typeof(TConfig), out var cachedSlot))
        {
            slot = cachedSlot as IPrototypeConfigSlot<TSubject, TConfig>;
            return slot is not null;
        }

        if (!ConfigSlotResolver.TryResolve<TConfig>(out var resolvedSlot))
        {
            slot = default;
            typedSlotsCache[typeof(TConfig)] = null;
            return false;
        }

        if (Slots.TryGetValue(resolvedSlot.Id, out var existingSlot))
        {
            if (existingSlot is not IPrototypeConfigSlot<TSubject, TConfig> typedSlot)
            {
                throw new InvalidOperationException(
                    $"An existing config slot for '{resolvedSlot.Id}' is of type {existingSlot.GetType().FullName}, which is not assignable to the requested type {typeof(IPrototypeConfigSlot<TSubject, TConfig>).FullName}."
                );
            }
            slot = typedSlot;
        }
        else
        {
            Slots[resolvedSlot.Id] = resolvedSlot;
            slot = resolvedSlot;
        }

        typedSlotsCache[typeof(TConfig)] = slot;
        return true;
    }

    bool IPrototype.TryGetSlot<TConfig>([MaybeNullWhen(false)] out IPrototypeConfigSlotOf<TConfig> slot)
    {
        if (TryGetSlot<TConfig>(out var typedSlot))
        {
            slot = typedSlot;
            return true;
        }

        slot = default;
        return false;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{GetType().Name}<{SubjectType.Name}>#{Id}";
    }
}
