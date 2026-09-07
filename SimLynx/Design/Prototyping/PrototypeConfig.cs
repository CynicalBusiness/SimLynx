using System.Collections.Generic;
using SimLynx.Design.Prototyping.Blueprints;
using SimLynx.Discovery.Content;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Base class for prototype configs.
/// </summary>
/// <typeparam name="TSubject">The type of the prototype subject.</typeparam>
/// <param name="name">The name of this configuration.</param>
public abstract class PrototypeConfig<TSubject>(Identifier name) : IPrototypeConfig<TSubject>
    where TSubject : class, IPrototypeSubject
{
    /// <inheritdoc/>
    public Identifier Name { get; } = name;

    /// <inheritdoc/>
    public virtual bool IsEmpty => !HasReset && Attributions.Count == 0;

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
        AttributeCurrent();
    }

    /// <inheritdoc/>
    public virtual void Attribute(IContentProvider provider)
    {
        Attributions.Add(provider);
    }

    /// <inheritdoc/>
    public virtual void CopyTo(IPrototypeConfig other)
    {
        if (HasReset)
        {
            other.Reset();
        }

        foreach (var attribution in Attributions)
        {
            other.Attribute(attribution);
        }
    }

    /// <inheritdoc/>
    public virtual void Validate()
    {
        // no-op by default
    }

    /// <summary>
    /// Applies the current content attribution context to this config.
    /// </summary>
    protected virtual void AttributeCurrent()
    {
        Attribute(ContentAttributionRegistry.Current.GetForCaller());
    }
}
