using System.Collections.Generic;
using System.Collections.Immutable;
using Autofac.Core;
using SimLynx.Discovery.Content;

namespace SimLynx.Design.Prototyping.Blueprints;

/// <summary>
/// Builder used by configs to construct a blueprint for a given subject type.
/// </summary>
/// <typeparam name="TSubject">The type of the subject for which the blueprint is being built.</typeparam>
public interface IBlueprintBuilder<out TSubject>
    where TSubject : class, IPrototypeSubject
{
    /// <summary>
    /// Context options for all build operations, copied into each build's <see cref="IBlueprintBuildContext.InstanceOptions"/>.
    /// </summary>
    public ITypeDictionary<object> Options { get; }

    /// <summary>
    /// The list of injection parameters to be used when creating a new instance of the subject, prepended to those
    /// returned by the handlers registered with <see cref="ConfigureBeforeCreate(BlueprintPreCreateHandler)"/>.
    /// </summary>
    public List<Parameter> InjectionParameters { get; }

    /// <summary>
    /// The prototype this builder is building a blueprint for.
    /// </summary>
    public IPrototype<TSubject> Prototype { get; }

    /// <summary>
    /// Gets a deduplicated enumeration of all attributions for this prototype and its ancestors, starting from the root prototype and ending with this prototype, in order of application.
    /// </summary>
    public ImmutableArray<IContentProvider> Attributions { get; }

    /// <summary>
    /// The action to be invoked after a new instance of the subject is created.
    /// </summary>
    /// <param name="handler">The handler to be invoked.</param>
    public void ConfigureAfterCreate(BlueprintPostCreateHandler<TSubject> handler);

    /// <summary>
    /// Configures the blueprint to invoke the given <paramref name="handler"/> before each creation attempt but before the subject is created.
    /// </summary>
    /// <param name="handler">The handler to be invoked.</param>
    public void ConfigureBeforeCreate(BlueprintPreCreateHandlerWithParams handler);

    /// <inheritdoc cref="ConfigureBeforeCreate(BlueprintPreCreateHandlerWithParams)"/>
    public void ConfigureBeforeCreate(BlueprintPreCreateHandler handler);

    /// <summary>
    /// Builds the blueprint.
    /// </summary>
    /// <returns>The built blueprint</returns>
    public IBlueprint<TSubject> Build();
}
