using System.Collections.Generic;
using SimLynx.Design.Prototyping.Blueprints;
using SimLynx.Design.Prototyping.Properties;
using SimLynx.Discovery.Content;

namespace SimLynx.Design.Prototyping.Facets;

/// <inheritdoc cref="IFacetPropertyConfig{TSubject, TFacet}"/>
public class FacetPropertyConfig<TSubject, TFacet>(IPropertyConfig<TFacet> property)
    : IFacetPropertyConfig<TSubject, TFacet>
    where TSubject : class, IPrototypeSubject
    where TFacet : class, IPrototypeFacet
{
    /// <inheritdoc/>
    public IPropertyConfig<TFacet> Property { get; } = property;

    /// <inheritdoc/>
    public Identifier Name => Property.Name;

    /// <inheritdoc/>
    public bool IsEmpty => Property.IsEmpty;

    /// <inheritdoc/>
    public IEnumerable<IContentProvider> Attributions => Property.Attributions;

    IPropertyConfig IFacetPropertyConfig.Property => Property;

    /// <inheritdoc/>
    public bool Apply(IBlueprintBuilder<TSubject> builder)
    {
        return false;
    }

    /// <inheritdoc/>
    public void Clear()
    {
        Property.Clear();
    }

    /// <inheritdoc/>
    public void Reset()
    {
        Property.Reset();
    }
}
