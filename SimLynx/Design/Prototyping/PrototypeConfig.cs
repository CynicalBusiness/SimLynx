using System.Collections.Generic;
using SimLynx.Design.Prototyping.Blueprints;
using SimLynx.Discovery.Content;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Base class for prototype configs.
/// </summary>
/// <typeparam name="TSubject"></typeparam>
/// <param name="name"></param>
public abstract class PrototypeConfig<TSubject>(string name) : IPrototypeConfig<TSubject>
    where TSubject : class, IPrototypeSubject
{
    /// <inheritdoc/>
    public string Name { get; } = name;

    /// <inheritdoc/>
    public abstract bool IsEmpty { get; }

    /// <summary>
    /// Indicates whether this config has been explicitly reset to its default state.
    /// </summary>
    public virtual bool HasReset { get; protected set; } = false;

    /// <inheritdoc cref="IPrototypeConfig.Attributions"/>
    public LinkedHashSet<IContentProvider> Attributions { get; } = [];

    IEnumerable<IContentProvider> IPrototypeConfig.Attributions => Attributions;

    /// <inheritdoc/>
    public abstract bool Apply(IBlueprintBuilder<TSubject> builder);

    /// <inheritdoc/>
    public virtual void Clear()
    {
        HasReset = false;
        Attributions.Clear();
    }

    /// <inheritdoc/>
    public virtual void Reset()
    {
        Clear();
        HasReset = true;
        ApplyAttribution();
    }

    /// <summary>
    /// Applies the current content attribution context to this config.
    /// </summary>
    protected virtual void ApplyAttribution()
    {
        Attributions.Add(ContentAttributionRegistry.Current.GetForCaller());
    }
}
