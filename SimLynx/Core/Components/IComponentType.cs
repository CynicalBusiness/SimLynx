
using System;
using Autofac;

namespace SimLynx.Core.Components;

/// <summary>
/// Generalized interface for metadata about a type of component.
/// </summary>
public interface IComponentType
{

    /// <summary>
    /// The system type of the component that this metadata describes.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Creates a default instance of the component.
    /// </summary>
    /// <param name="builder">The builder to pass to the constructor</param>
    /// <param name="scope">The injection scope to create the component in</param>
    /// <returns>A new instance of the component</returns>
    public Component CreateDefault(ComponentBuilder builder, ILifetimeScope scope);

}
