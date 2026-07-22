using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Autofac.Features.AttributeFilters;
using SimLynx.Design.Prototyping.Blueprints;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Registry of prototypes.
/// </summary>
/// <typeparam name="TBaseSubject">The base type of the subject for which this registry is storing prototypes.</typeparam>
public class PrototypeRegistry<TBaseSubject>(PrototypeResolver<TBaseSubject> resolver)
    : IReadOnlyDictionary<Symbol, IPrototype<TBaseSubject>>
    where TBaseSubject : class, IPrototypeSubject
{
    /// <summary>
    /// Delegate for handling events related to prototypes in this registry.
    /// </summary>
    /// <param name="prototype">The prototype associated with the event.</param>
    public delegate void PrototypeEventHandler(IPrototype<TBaseSubject> prototype);

    /// <summary>
    /// Event that is raised when a new prototype is added in this registry.
    /// </summary>
    public event PrototypeEventHandler? OnPrototypeAdded;

    private readonly Dictionary<Symbol, IPrototype<TBaseSubject>> _prototypes = [];

    /// <summary>
    /// Gets the prototype for the given prototype <paramref name="key"/>, throwing an exception if no such prototype exists.
    /// </summary>
    /// <param name="key">The ID of the relevant prototype.</param>
    /// <returns>The prototype associated with the given ID, or <c>null</c> if no such prototype exists.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no prototype exists with the given <paramref name="key"/>.</exception>
    public IPrototype<TBaseSubject> this[Symbol key] => _prototypes[key];

    /// <inheritdoc/>
    public IEnumerable<Symbol> Keys => _prototypes.Keys;

    /// <inheritdoc/>
    public IEnumerable<IPrototype<TBaseSubject>> Values => _prototypes.Values;

    /// <inheritdoc/>
    public int Count => _prototypes.Count;

    /// <inheritdoc/>
    public bool ContainsKey(Symbol key)
    {
        return _prototypes.ContainsKey(key);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<Symbol, IPrototype<TBaseSubject>>> GetEnumerator()
    {
        return _prototypes.GetEnumerator();
    }

    /// <summary>
    /// Tries to get the prototype for the given prototype <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The ID of the relevant prototype.</param>
    /// <param name="prototype">The prototype associated with the given ID, if found.</param>
    /// <returns><c>true</c> if the prototype was found; otherwise, <c>false</c>.</returns>
    public bool TryGet(Symbol id, [MaybeNullWhen(false)] out IPrototype<TBaseSubject> prototype)
    {
        return _prototypes.TryGetValue(id, out prototype);
    }

    bool IReadOnlyDictionary<Symbol, IPrototype<TBaseSubject>>.TryGetValue(
        Symbol key,
        out IPrototype<TBaseSubject> value
    )
    {
        return TryGet(key, out value!);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Requests a prototype for further configuration, creating it if it does not already exist.
    /// </summary>
    /// <remarks>
    /// If the prototype <em>does not</em> exist, it is created with <typeparamref name="TSubject"/> as its subject type, and
    /// <paramref name="isAbstract"/> and the prototype registered as <paramref name="baseId"/> are applied to the new
    /// prototype, if provided.
    /// <br/>
    /// If the prototype <em>does</em> exist, it is returned for further configuration provided its subject type is
    /// assignable to <typeparamref name="TSubject"/>. In this case, <paramref name="isAbstract"/> and
    /// <paramref name="baseId"/> are invalid and will cause an exception to be thrown if provided.
    /// </remarks>
    /// <param name="id">ID of the prototype to configure.</param>
    /// <param name="isAbstract">Indicates whether the prototype should be abstract.</param>
    /// <param name="baseId">ID of the base prototype to inherit from.</param>
    /// <exception cref="ArgumentException">Thrown if the prototype cannot be configured as requested.</exception>
    public IPrototype<TSubject> Configure<TSubject>(
        Symbol id,
        Maybe<bool> isAbstract = default,
        Maybe<Symbol> baseId = default
    )
        where TSubject : class, TBaseSubject
    {
        if (TryGet(id, out var existingPrototype))
        {
            if (existingPrototype is not IPrototype<TSubject> prototype)
            {
                throw new ArgumentException(
                    $"{existingPrototype} exists for {id} but its subject type {existingPrototype.SubjectType} is not assignable from {typeof(TSubject)}.",
                    nameof(TSubject)
                );
            }

            if (isAbstract.HasValue)
            {
                throw new ArgumentException(
                    $"{prototype} exists for {id} and cannot have its abstractness changed.",
                    nameof(isAbstract)
                );
            }

            if (baseId.HasValue)
            {
                throw new ArgumentException(
                    $"{prototype} exists for {id} and cannot have its base prototype changed.",
                    nameof(baseId)
                );
            }

            return prototype;
        }

        var basePrototype = Maybe<IPrototype?>.None;
        if (baseId.HasValue)
        {
            if (!TryGet(baseId.Value, out var p))
            {
                throw new ArgumentException($"No prototype exists with ID '{baseId.Value}'", nameof(baseId));
            }

            basePrototype = new(p);
        }

        try
        {
            var prototype = resolver.Resolve<TSubject>(id, isAbstract, basePrototype);

            _prototypes[id] = prototype;
            OnPrototypeAdded?.Invoke(prototype);

            return prototype;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Failed to configure prototype with ID '{id}'", nameof(id), ex);
        }
    }

    /// <summary>
    /// Compiles all prototypes in this registry into a <see cref="BlueprintCatalog{TBaseSubject}"/>.
    /// </summary>
    /// <returns>The catalog of compiled blueprints.</returns>
    public BlueprintCatalog<TBaseSubject> Compile()
    {
        return new BlueprintCatalog<TBaseSubject>(Values.Where(p => !p.IsAbstract).Select(p => p.Compile()));
    }

    /// <summary>
    /// Factory for creating <see cref="PrototypeRegistry{TBaseSubject}"/> instances.
    /// </summary>
    /// <param name="factoryFunc">The DI-provided injected factory function.</param>
    public class Factory([KeyFilter(Factory.TRANSIENT_REGISTRY_NAME)] Func<PrototypeResolver<TBaseSubject>> factoryFunc)
    {
        /// <summary>
        /// Registration name for transient prototype registries, which can be used to create more registries of the same subject type.
        /// </summary>
        public const string TRANSIENT_REGISTRY_NAME = "new";

        /// <summary>
        /// Creates a new <see cref="PrototypeRegistry{TBaseSubject}"/> using the provided factory function.
        /// </summary>
        /// <returns>The newly created <see cref="PrototypeRegistry{TBaseSubject}"/>.</returns>
        public PrototypeRegistry<TBaseSubject> Create() => new(factoryFunc());
    }
}
