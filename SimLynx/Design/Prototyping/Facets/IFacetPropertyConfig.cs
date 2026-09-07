using SimLynx.Design.Prototyping.Properties;

namespace SimLynx.Design.Prototyping.Facets;

/// <summary>
/// A config for a particular property of a facet.
/// </summary>
public interface IFacetPropertyConfig : IPrototypeConfig
{
    /// <summary>
    /// The property config for the target property of this configuration.
    /// </summary>
    public IPropertyConfig Property { get; }
}

/// <summary>
/// A config for a particular property of a facet of a prototype's subject type.
/// </summary>
/// <typeparam name="TSubject">The subject type of the prototype to which the facet is attached</typeparam>
/// <typeparam name="TFacet">The type of the facet</typeparam>
public interface IFacetPropertyConfig<TSubject, TFacet> : IFacetPropertyConfig, IPrototypeConfig<TSubject>
    where TSubject : class, IPrototypeSubject
    where TFacet : class, IPrototypeFacet
{
    /// <inheritdoc cref="IFacetPropertyConfig.Property"/>
    public new IPropertyConfig<TFacet> Property { get; }

    IPropertyConfig IFacetPropertyConfig.Property => Property;
}
