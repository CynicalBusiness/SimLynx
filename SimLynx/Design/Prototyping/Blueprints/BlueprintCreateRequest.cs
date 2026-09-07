using Autofac;

namespace SimLynx.Design.Prototyping.Blueprints;

/// <summary>
/// Represents a request to create a new instance of a subject from a blueprint.
/// </summary>
/// <param name="scope">The Autofac scope used to resolve dependencies.</param>
public class BlueprintCreateRequest(ILifetimeScope scope)
{
    /// <summary>
    /// The Autofac scope used to resolve dependencies during the creation of a new subject instance.
    /// </summary>
    public ILifetimeScope Scope { get; } = scope;

    /// <summary>
    /// Additional context options to provide to this particular instance.
    /// </summary>
    public ITypeDictionary<object>? Options { get; init; }
}
