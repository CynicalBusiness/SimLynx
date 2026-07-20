using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace SimLynx.Core.Prototyping.Blueprints;

/// <summary>
/// Builder used by configs to construct a blueprint for a given subject type.
/// </summary>
/// <typeparam name="TSubject">The type of the subject for which the blueprint is being built.</typeparam>
public class BlueprintBuilder<TSubject>(IPrototype<TSubject> prototype)
    where TSubject : class, IPrototypeSubject
{
    /// <summary>
    /// Handler delegate for <see cref="OnBeforeCreate"/>.
    /// </summary>
    /// <param name="context">The context for the build operation.</param>
    /// <returns>Additional injection parameters to be used when creating the subject.</returns>
    public delegate IEnumerable<Parameter> PreCreateHandler(BuildContext context);

    /// <summary>
    /// Handler delegate for <see cref="OnCreate"/>.
    /// </summary>
    /// <param name="context">The context for the build operation.</param>
    /// <param name="subject">The newly-created subject instance.</param>
    public delegate void PostCreateHandler(BuildContext context, TSubject subject);

    /// <summary>
    /// The list of injection parameters to be used when creating a new instance of the subject.
    /// </summary>
    public List<Parameter> InjectionParameters = [];

    /// <summary>
    /// Context dictionary used to store state shared between configs.
    /// </summary>
    /// <remarks>
    /// This dictionary is shared between builds and is cloned into <see cref="BuildContext.InstanceOptions"/> for each
    /// build operation.
    /// </remarks>
    public TypeDictionary Options { get; } = [];

    /// <summary>
    /// Action invoked on each creation attempt but before the subject is created.
    /// </summary>
    public PreCreateHandler? OnBeforeCreate { get; set; }

    /// <summary>
    /// The action to be invoked after a new instance of the subject is created.
    /// </summary>
    public PostCreateHandler? OnCreate { get; set; }

    /// <summary>
    /// The prototype for which the blueprint is being built.
    /// </summary>
    public IPrototype<TSubject> Prototype { get; } = prototype;

    /// <summary>
    /// Builds the blueprint.
    /// </summary>
    /// <returns>The constructed blueprint for the subject type.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public IBlueprint<TSubject> Build()
    {
        return new Blueprint<TSubject>(this);
    }

    /// <summary>
    /// Context for a build operation.
    /// </summary>
    /// <param name="Scope">The Autofac scope used to resolve dependencies.</param>
    /// <param name="InstanceOptions">The instance-specific context dictionary for resolving dependencies.</param>
    public record BuildContext(TypeDictionary InstanceOptions, ILifetimeScope Scope);
}
