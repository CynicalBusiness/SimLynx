using System;
using SimLynx.Design.Prototyping.Blueprints;
using SimLynx.Design.Prototyping.Properties;

namespace SimLynx.Design.Prototyping.Facets;

/// <summary>
/// A config slot for defining "facets", which are separate statically-known objects attached to a prototype and
/// can augment the prototype's behavior or configuration.
/// </summary>
/// <remarks>
/// To define a facet, subclass this slot and close <typeparamref name="TFacet"/> to the type of the facet, leaving
/// <typeparamref name="TSubject"/> as a open generic to be closed by the prototype.
/// <br/>
/// That is: <c>MyFacetConfigSlot&lt;TSubject&gt; : FacetConfigSlot&lt;TSubject, MyFacet&gt;</c> where
/// <c>MyFacet</c> is the type of the facet.
/// <br/>
/// Facet slots will prototype their facet instance before the the main prototype and store it in the build context,
/// keyed by <see cref="ContextKey"/> (which defaults to facet type + slot ID), for use by other configs.
/// </remarks>
/// <typeparam name="TSubject">The subject of the prototype to which the facet is attached</typeparam>
/// <typeparam name="TFacet">The type of the facet</typeparam>
public abstract class FacetConfigSlot<TSubject, TFacet>
    : PrototypeConfigSlot<TSubject, IFacetPropertyConfig<TSubject, TFacet>>
    where TSubject : class, IPrototypeSubject
    where TFacet : class, IPrototypeFacet
{
    /// <summary>
    /// Creates a new facet config slot.
    /// </summary>
    /// <param name="prototype">The prototype to which the facet is attached</param>
    /// <param name="context">The context to use for the inner facet prototype.</param>
    public FacetConfigSlot(IPrototype<TSubject> prototype, IPrototypeContext context)
        : base(prototype)
    {
        var baseFacetPrototype = prototype.Base?[Id] is FacetConfigSlot<TSubject, TFacet> baseSlot
            ? baseSlot.FacetPrototype
            : null;
        FacetPrototype = new(Id, context) { Base = baseFacetPrototype };
    }

    /// <summary>
    /// The prototype used to configure the facet's instance.
    /// </summary>
    public FacetPrototype<TFacet> FacetPrototype { get; }

    /// <summary>
    /// The type key used to store the facet instance in the build context.
    /// </summary>
    public virtual TypeKey<TFacet> ContextKey => new(Id);

    /// <inheritdoc/>
    public override void Configure(IBlueprintBuilder<TSubject> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        var facetBlueprint = FacetPrototype.Compile();
        builder.ConfigureBeforeCreate(
            (ctx) =>
            {
                var facet = facetBlueprint.CreateInstance(ctx.Scope);
                ctx.InstanceOptions.Set(ContextKey, facet);
            }
        );
    }

    /// <inheritdoc/>
    protected override IFacetPropertyConfig<TSubject, TFacet>? Create(Identifier id)
    {
        var propConfig = FacetPrototype.Properties.Get(id);
        if (propConfig is null)
        {
            return null;
        }

        return new FacetPropertyConfig<TSubject, TFacet>(propConfig);
    }
}
