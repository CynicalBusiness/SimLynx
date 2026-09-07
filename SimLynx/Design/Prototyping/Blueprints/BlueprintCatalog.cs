using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SimLynx.Design.Prototyping.Blueprints;

/// <summary>
/// Read-only catalog of blueprints for <typeparamref name="TBaseSubject"/> blueprints.
/// </summary>
/// <typeparam name="TBaseSubject">The base subject type.</typeparam>
/// <param name="blueprints">The collection of blueprints to include in the catalog.</param>
public class BlueprintCatalog<TBaseSubject>(IEnumerable<IBlueprint<TBaseSubject>> blueprints)
    : IReadOnlyDictionary<Identifier, IBlueprint<TBaseSubject>>
    where TBaseSubject : class, IPrototypeSubject
{
    private readonly Dictionary<Identifier, IBlueprint<TBaseSubject>> _blueprints = blueprints.ToDictionary(b =>
        b.Prototype.Name
    );

    /// <summary>
    /// Gets the blueprint for the given prototype <paramref name="id"/>, or <c>null</c> if no such blueprint exists.
    /// </summary>
    /// <param name="id">The ID of the relevant prototype.</param>
    /// <returns>The blueprint associated with the given prototype ID, or <c>null</c> if no such blueprint exists.</returns>
    public IBlueprint<TBaseSubject>? this[Identifier id] => TryGet(id, out var blueprint) ? blueprint : null;

    IBlueprint<TBaseSubject> IReadOnlyDictionary<Identifier, IBlueprint<TBaseSubject>>.this[Identifier key] =>
        _blueprints[key];

    /// <inheritdoc/>
    public IEnumerable<Identifier> Keys => _blueprints.Keys;

    /// <summary>
    /// Enumeration of all blueprints in this catalog.
    /// </summary>
    public IEnumerable<IBlueprint<TBaseSubject>> Blueprints => _blueprints.Values;

    IEnumerable<IBlueprint<TBaseSubject>> IReadOnlyDictionary<Identifier, IBlueprint<TBaseSubject>>.Values =>
        _blueprints.Values;

    /// <inheritdoc/>
    public int Count => _blueprints.Count;

    /// <summary>
    /// Determines whether this catalog contains a blueprint for the given prototype <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The ID of the relevant prototype.</param>
    /// <returns><c>true</c> if the blueprint exists; otherwise, <c>false</c>.</returns>
    public bool Contains(Identifier id) => _blueprints.ContainsKey(id);

    bool IReadOnlyDictionary<Identifier, IBlueprint<TBaseSubject>>.ContainsKey(Identifier key) =>
        _blueprints.ContainsKey(key);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<Identifier, IBlueprint<TBaseSubject>>> GetEnumerator() =>
        _blueprints.GetEnumerator();

    /// <inheritdoc/>
    bool IReadOnlyDictionary<Identifier, IBlueprint<TBaseSubject>>.TryGetValue(
        Identifier key,
        out IBlueprint<TBaseSubject> value
    ) => _blueprints.TryGetValue(key, out value!);

    /// <summary>
    /// Tries to get the blueprint for the given prototype <paramref name="id"/>.
    /// </summary>
    /// <param name="id">ID of the relevant prototype.</param>
    /// <param name="blueprint">The blueprint associated with the given prototype ID, if found.</param>
    /// <returns><c>true</c> if the blueprint was found; otherwise, <c>false</c>.</returns>
    public bool TryGet(Identifier id, [MaybeNullWhen(false)] out IBlueprint<TBaseSubject> blueprint)
    {
        return _blueprints.TryGetValue(id, out blueprint);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
