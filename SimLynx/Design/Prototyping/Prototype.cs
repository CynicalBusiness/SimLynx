using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SimLynx.Design.Prototyping.Blueprints;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Standard base implementation of a prototype.
/// </summary>
/// <remarks>
/// Contains default implementations and helpers suitable for most prototypes.
/// </remarks>
public class Prototype<TSubject> : IPrototype<TSubject>
    where TSubject : class, IPrototypeSubject
{
    private readonly Dictionary<Type, IPrototypeConfigSlot<TSubject, IPrototypeConfig>?> typedSlotsCache = [];
    private readonly IPrototypeContext context;

    /// <summary>
    /// Dictionary of config slots for this prototype, keyed by their slot ID.
    /// </summary>
    protected Dictionary<Identifier, IPrototypeConfigSlot<TSubject, IPrototypeConfig>> Slots { get; } = [];

    IEnumerable<IPrototypeConfigSlot> IPrototype.Slots => Slots.Values;

    /// <summary>
    /// Creates a new prototype with the given <paramref name="name"/> and <paramref name="context"/>.
    /// </summary>
    /// <param name="name">The name of the prototype.</param>
    /// <param name="context">The context in which the prototype exists.</param>
    public Prototype(Identifier name, IPrototypeContext context)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        Name = name;
        this.context = context;

        foreach (var entry in context.ConfigSlotCatalog.Entries.Where(e => e.Def.Eager))
        {
            // just try to add it, letting it fail silently if not supported/found
            TryGetSlot(entry.Def.Id, out _);
        }
    }

    /// <inheritdoc/>
    public Identifier Name { get; }

    /// <inheritdoc cref="IPrototype.Base"/>
    public IPrototype? Base
    {
        get => field;
        init
        {
            // this check can be fairly simple since we can make a few assumptions about the base prototype:
            // - has already validated its own base prototype, if any
            // - is init-only and should not be able to cycle (baring reflection shenanigans, which we don't need to defend against)

            if (value is not null && !value.SubjectType.IsAssignableFrom(SubjectType))
            {
                throw new ArgumentException(
                    $"Base prototype '{value.Name}' is of type {value.SubjectType}, which is not a base type of this prototype's subject type: {SubjectType}.",
                    nameof(value)
                );
            }

            field = value;
        }
    }

    /// <inheritdoc/>
    public virtual bool IsAbstract { get; init; } = false;

    /// <inheritdoc/>
    public Type SubjectType => typeof(TSubject);

    /// <summary>
    /// Gets the currently-defined config, if any, for the given <paramref name="slotId"/> and <paramref name="configName"/>.
    /// </summary>
    /// <param name="slotId">The slot for which to retrieve the config.</param>
    /// <param name="configName">The name of the config to retrieve.</param>
    /// <returns>The config if found; otherwise, <c>null</c>.</returns>
    public IPrototypeConfig? this[Identifier slotId, Identifier configName] => this[slotId]?[configName];

    /// <inheritdoc cref="IPrototype.this[Identifier]"/>
    public IPrototypeConfigSlot<TSubject, IPrototypeConfig>? this[Identifier slotId] => Slots.GetValueOrDefault(slotId);

    IPrototypeConfigSlot? IPrototype.this[Identifier slotId] => this[slotId];

    /// <inheritdoc/>
    public virtual IBlueprint<TSubject> Compile()
    {
        if (IsAbstract)
        {
            throw new InvalidOperationException($"Cannot compile an abstract prototype: {this}");
        }

        var builder = new BlueprintBuilder<TSubject>(this);
        Configure(builder);
        return builder.Build();
    }

    /// <summary>
    /// Applies this prototype's configuration to the given <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The builder to apply to</param>
    public virtual void Configure(IBlueprintBuilder<TSubject> builder)
    {
        var slots = GetAllSlots().ToList(); // eagerly materialize
        foreach (var slot in slots)
        {
            slot.Configure(builder);
        }
    }

    /// <summary>
    /// Gets an effective enumeration of config slots for this prototype, including inherited slots.
    /// </summary>
    /// <remarks>
    /// Slots are ordered by their priority, with higher-priority slots appearing first, with ties broken by
    /// <em>DI container</em> registration order.
    /// </remarks>
    /// <returns>An enumerable of effective config slots for this prototype.</returns>
    protected virtual IEnumerable<IPrototypeConfigSlotFor<TSubject>> GetAllSlots()
    {
        return this.GetAncestors(includeSelf: true)
            .SelectMany(prototype => prototype.Slots)
            .Cast<IPrototypeConfigSlotFor<TSubject>>()
            .Distinct(PrototypeConfigSlotIdComparer<IPrototypeConfigSlotFor<TSubject>>.Default)
            .OrderBy(slot => context.ConfigSlotCatalog.EntriesById[slot.Id].SortIndex);
    }

    /// <inheritdoc cref="IPrototype.TryGetSlot"/>
    public bool TryGetSlot(
        Identifier slotId,
        [MaybeNullWhen(false)] out IPrototypeConfigSlot<TSubject, IPrototypeConfig> slot
    )
    {
        if (!Slots.TryGetValue(slotId, out slot))
        {
            if (
                context.ConfigSlotCatalog.TryResolve(slotId, this, out var resolvedSlot)
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

    bool IPrototype.TryGetSlot(Identifier slotId, [MaybeNullWhen(false)] out IPrototypeConfigSlot slot)
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

        if (!context.ConfigSlotCatalog.TryResolve<TSubject, TConfig>(this, out var resolvedSlot))
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
        return $"{GetType().Name}<{SubjectType.Name}>{Name}";
    }
}
