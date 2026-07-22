using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace SimLynx.Design.Prototyping.Blueprints;

/// <summary>
/// Builder used by configs to construct a blueprint for a given subject type.
/// </summary>
/// <typeparam name="TSubject">The type of the subject for which the blueprint is being built.</typeparam>
public class BlueprintBuilder<TSubject>(IPrototype<TSubject> prototype) : IBlueprintBuilder<TSubject>
    where TSubject : class, IPrototypeSubject
{
    /// <inheritdoc/>
    public List<Parameter> InjectionParameters { get; } = [];

    /// <inheritdoc/>
    public TypeDictionary Options { get; } = [];

    /// <summary>
    /// Action invoked on each creation attempt but before the subject is created.
    /// </summary>
    public List<BlueprintPreCreateHandler> BeforeCreateHandlers { get; } = [];

    /// <summary>
    /// The list of actions to be invoked after a new instance of the subject is created.
    /// </summary>
    public List<BlueprintPostCreateHandler<TSubject>> AfterCreateHandlers { get; } = [];

    /// <inheritdoc/>
    public IPrototype<TSubject> Prototype { get; } = prototype;

    /// <inheritdoc/>
    public IBlueprint<TSubject> Build()
    {
        return new Blueprint<TSubject>(this);
    }

    /// <inheritdoc/>
    public void ConfigureBeforeCreate(BlueprintPreCreateHandler handler)
    {
        BeforeCreateHandlers.Add(handler);
    }

    /// <inheritdoc/>
    public void ConfigureAfterCreate(BlueprintPostCreateHandler<TSubject> handler)
    {
        AfterCreateHandlers.Add(handler);
    }

    /// <summary>
    /// Context for a build operation.
    /// </summary>
    /// <param name="Scope">The Autofac scope used to resolve dependencies.</param>
    /// <param name="InstanceOptions">The instance-specific context dictionary for resolving dependencies.</param>
    /// <param name="SubjectType">The concrete type of subject being built.</param>
    public record BuildContext(TypeDictionary InstanceOptions, ILifetimeScope Scope, Type SubjectType)
        : IBlueprintBuildContext;
}
