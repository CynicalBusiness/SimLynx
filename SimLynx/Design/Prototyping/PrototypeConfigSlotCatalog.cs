using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Autofac;
using Autofac.Core;
using SimLynx.Core;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Catalog of prototype configuration slots. Can be used to resolve slot instances and metadata about them.
/// </summary>
public class PrototypeConfigSlotCatalog
{
    /// <summary>
    /// Creates a new catalog of prototype configuration slots.
    /// </summary>
    /// <remarks>
    /// Definitions are stable-sorted by descending priority for slot configuration; equal-priority definitions retain
    /// <paramref name="slotDefs"/> order. Slot IDs must be unique. Config-type aliases may be shared by multiple
    /// definitions and retain their original registration order independently of priority.
    /// </remarks>
    /// <param name="slotDefs">Slot definitions to include in the catalog.</param>
    /// <param name="container">The Autofac container used to resolve slot instances.</param>
    public PrototypeConfigSlotCatalog(IEnumerable<PrototypeConfigSlotDef> slotDefs, IComponentContext container)
    {
        Entries =
        [
            .. slotDefs
                .Select((def, i) => (def, i))
                .OrderBy(tuple => tuple.def.Priority, PriorityComparer.Default)
                .Select((tuple, i) => new Entry(tuple.def, container, sortIndex: i, registrationIndex: tuple.i)),
        ];

        EntriesById = Entries.ToDictionary(entry => entry.Def.Id, entry => entry);
        EntriesByConfigType = Entries
            .SelectMany(entry => entry.Def.ConfigTypes, (entry, configType) => (entry, configType))
            .GroupBy(tuple => tuple.configType, tuple => tuple.entry)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<Entry>)[.. group.OrderBy(entry => entry.RegistrationIndex).Reverse()]
            );
    }

    /// <summary>
    /// List of all entries in this catalog, stable-sorted by descending priority and then registration order.
    /// </summary>
    public IReadOnlyList<Entry> Entries { get; }

    /// <summary>
    /// Dictionary of all entries in this catalog, keyed by their unique slot ID.
    /// </summary>
    public IReadOnlyDictionary<Symbol, Entry> EntriesById { get; }

    /// <summary>
    /// Dictionary of all entries in this catalog, grouped by config types they support.
    /// </summary>
    /// <remarks>
    /// Each candidate list is ordered by reverse registration order and does not use slot priority.
    /// </remarks>
    public IReadOnlyDictionary<Type, IReadOnlyList<Entry>> EntriesByConfigType { get; }

    /// <summary>
    /// Attempts to resolve a config slot for the given <paramref name="slotId"/> and <paramref name="prototype"/>.
    /// If the slot is not found, or is not supported for the provided <paramref name="prototype"/>, returns false.
    /// </summary>
    /// <typeparam name="TSubject"></typeparam>
    /// <param name="slotId"></param>
    /// <param name="prototype"></param>
    /// <param name="slot"></param>
    /// <returns></returns>
    public bool TryResolve<TSubject>(
        Symbol slotId,
        IPrototype<TSubject> prototype,
        [MaybeNullWhen(false)] out IPrototypeConfigSlotFor<TSubject> slot
    )
        where TSubject : class, IPrototypeSubject
    {
        if (EntriesById.TryGetValue(slotId, out var entry))
        {
            return entry.TryResolve(prototype, out slot);
        }

        slot = null;
        return false;
    }

    /// <summary>
    /// Attempts to resolve all config slots for the given <paramref name="configType"/> which are supported for
    /// the given <paramref name="prototype"/>.
    /// If no slots are found, or none are supported for the provided <paramref name="prototype"/>, returns an empty
    /// enumeration.
    /// </summary>
    /// <remarks>
    /// Exact closed-generic aliases are enumerated before aliases for the corresponding open-generic type definition.
    /// Within either specificity group, candidates are tried in reverse registration order; priority is ignored.
    /// Candidates that cannot be closed by Autofac for <typeparamref name="TSubject"/>, or whose constructed slot
    /// reports <see cref="IPrototypeConfigSlot.IsSupported"/> as false, are skipped.
    /// </remarks>
    /// <typeparam name="TSubject">The type of the prototype subject.</typeparam>
    /// <param name="prototype">The prototype instance for which to resolve the config slots.</param>
    /// <param name="configType">The type of the config slots to resolve.</param>
    /// <returns>An enumeration of resolved config slots that are supported for the given prototype.</returns>
    public IEnumerable<IPrototypeConfigSlotFor<TSubject>> ResolveByType<TSubject>(
        IPrototype<TSubject> prototype,
        Type configType
    )
        where TSubject : class, IPrototypeSubject
    {
        IEnumerable<Entry> entries = [];
        if (EntriesByConfigType.TryGetValue(configType, out var closedTypeEntries))
        {
            entries = entries.Concat(closedTypeEntries);
        }

        if (
            configType.IsGenericType
            && EntriesByConfigType.TryGetValue(configType.GetGenericTypeDefinition(), out var openTypeEntries)
        )
        {
            entries = entries.Concat(openTypeEntries);
        }

        foreach (var entry in entries)
        {
            if (entry.TryResolve(prototype, out var slot) && slot.IsSupported)
            {
                yield return slot;
            }
        }
    }

    /// <inheritdoc cref="ResolveByType{TSubject}(IPrototype{TSubject}, Type)"/>
    /// <typeparam name="TSubject">The type of the prototype subject.</typeparam>
    /// <typeparam name="TConfig">The type of the config slots to resolve.</typeparam>
    public IEnumerable<IPrototypeConfigSlot<TSubject, TConfig>> ResolveByType<TSubject, TConfig>(
        IPrototype<TSubject> prototype
    )
        where TSubject : class, IPrototypeSubject
        where TConfig : class, IPrototypeConfig
    {
        return ResolveByType(prototype, typeof(TConfig)).OfType<IPrototypeConfigSlot<TSubject, TConfig>>();
    }

    /// <summary>
    /// Attempts to resolve the first config slot for the given <paramref name="configType"/> and
    /// <paramref name="prototype"/> according to config-type alias precedence. If no slot is found or supported,
    /// returns false.
    /// </summary>
    /// <typeparam name="TSubject">The type of the prototype subject to resolve for.</typeparam>
    /// <param name="prototype">The prototype instance for which to resolve the config slot.</param>
    /// <param name="configType">The type of the config slot to resolve.</param>
    /// <param name="slot">The resolved config slot, if found and supported; otherwise, null.</param>
    /// <returns>True if the config slot was successfully resolved and is supported; otherwise, false.</returns>
    public bool TryResolve<TSubject>(
        IPrototype<TSubject> prototype,
        Type configType,
        [MaybeNullWhen(false)] out IPrototypeConfigSlotFor<TSubject> slot
    )
        where TSubject : class, IPrototypeSubject
    {
        slot = ResolveByType(prototype, configType).FirstOrDefault();
        return slot is not null;
    }

    /// <summary>
    /// Attempts to resolve the first config slot for the given <paramref name="prototype"/> and
    /// <typeparamref name="TConfig"/> according to config-type alias precedence. If no slot is found or supported,
    /// returns false.
    /// </summary>
    /// <typeparam name="TSubject">The type of the prototype subject to resolve for.</typeparam>
    /// <typeparam name="TConfig">The type of the config slot to resolve.</typeparam>
    /// <param name="prototype">The prototype instance for which to resolve the config slot.</param>
    /// <param name="slot">The resolved config slot, if found and supported; otherwise, null.</param>
    /// <returns>True if the config slot was successfully resolved and is supported; otherwise, false.</returns>
    public bool TryResolve<TSubject, TConfig>(
        IPrototype<TSubject> prototype,
        [MaybeNullWhen(false)] out IPrototypeConfigSlot<TSubject, TConfig> slot
    )
        where TSubject : class, IPrototypeSubject
        where TConfig : class, IPrototypeConfig
    {
        slot = ResolveByType<TSubject, TConfig>(prototype).FirstOrDefault();
        return slot is not null;
    }

    /// <summary>
    /// Entry in a slot catalog.
    /// </summary>
    /// <param name="def">The slot definition.</param>
    /// <param name="container">The Autofac container used to resolve slot instances.</param>
    /// <param name="sortIndex">The sort index of this entry in the catalog.</param>
    /// <param name="registrationIndex">The index of the order slots were registered.</param>
    public class Entry(PrototypeConfigSlotDef def, IComponentContext container, int sortIndex, int registrationIndex)
    {
        /// <summary>
        /// The slot definition for this entry.
        /// </summary>
        public PrototypeConfigSlotDef Def { get; } = def;

        /// <summary>
        /// The sorting index of this entry in the catalog, determining the order in which slots are configured.
        /// </summary>
        public int SortIndex { get; } = sortIndex;

        /// <summary>
        /// The index of the order slots were registered.
        /// </summary>
        public int RegistrationIndex { get; } = registrationIndex;

        /// <summary>
        /// Attempts to resolve a config slot for the given <paramref name="prototype"/>. If the slot is unavailable or
        /// contextually unsupported for the provided <paramref name="prototype"/>, returns false.
        /// </summary>
        /// <remarks>
        /// Autofac treats an open-generic registration whose constraints reject <typeparamref name="TSubject"/> as
        /// unavailable. Failures while activating an otherwise compatible registration propagate; they are not
        /// interpreted as an unsupported slot. Context-specific applicability is expressed by
        /// <see cref="IPrototypeConfigSlot.IsSupported"/> after successful activation.
        /// </remarks>
        /// <typeparam name="TSubject">The type of the prototype subject to resolve for.</typeparam>
        /// <param name="prototype">The prototype instance for which to resolve the config slot.</param>
        /// <param name="slot">The resolved config slot, if found and supported; otherwise, null.</param>
        /// <returns>True if the config slot was successfully resolved and is supported; otherwise, false.</returns>
        public bool TryResolve<TSubject>(
            IPrototype<TSubject> prototype,
            [MaybeNullWhen(false)] out IPrototypeConfigSlotFor<TSubject> slot
        )
            where TSubject : class, IPrototypeSubject
        {
            var serviceType = new KeyedService(Def.Id, typeof(IPrototypeConfigSlotFor<TSubject>));
            if (
                container.TryResolveService(
                    serviceType,
                    [new CovariantTypedParameter(prototype, ceilingType: typeof(IPrototype<TSubject>))],
                    out var rawSlot
                )
            )
            {
                slot = (IPrototypeConfigSlotFor<TSubject>)rawSlot;
                return slot.IsSupported;
            }

            slot = null;
            return false;
        }
    }
}
