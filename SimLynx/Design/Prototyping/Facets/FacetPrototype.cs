namespace SimLynx.Design.Prototyping.Facets;

/// <summary>
/// Prototype used to configure a facet of another prototype.
/// </summary>
/// <typeparam name="TFacet"></typeparam>
/// <param name="name"></param>
/// <param name="context"></param>
public class FacetPrototype<TFacet>(Identifier name, IPrototypeContext context) : Prototype<TFacet>(name, context)
    where TFacet : class, IPrototypeFacet { }
