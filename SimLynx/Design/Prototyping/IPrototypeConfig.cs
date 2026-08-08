using System.Collections.Generic;
using System.ComponentModel;
using SimLynx.Design.Prototyping.Blueprints;
using SimLynx.Discovery.Content;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// A single configuration for a prototype.
/// </summary>
public interface IPrototypeConfig
{
    /// <summary>
    /// The name of this configuration, unique for the config's slot.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Whether this configuration is empty, meaning it has no effect on the prototype or its blueprint.
    /// </summary>
    public bool IsEmpty { get; }

    /// <summary>
    /// The content providers attributed to having configured this config, if any.
    /// </summary>
    public IEnumerable<IContentProvider> Attributions { get; }

    /// <summary>
    /// Clears this config.
    /// </summary>
    /// <remarks>
    /// This method only clears configuration (e.g. whatever would make <see cref="IsEmpty"/> <c>true</c>), but does
    /// not affect other configs for the same feature (such as on base prototypes).
    /// <br/>
    /// To reset the feature to its default state irrespective of those configs, use <see cref="Reset"/> instead.
    /// <br/>
    /// For some implementations, the "reset" state may be cleared by this method.
    /// </remarks>
    public void Clear();

    /// <summary>
    /// Explicitly resets the feature configured by this config to its default state.
    /// </summary>
    /// <remarks>
    /// This method indicates that the feature should begin again in its default state, irrespective of other configs
    /// for the feature (such as on base prototypes), as if it had never been configured.
    /// <br/>
    /// To simply clear this particular config, use <see cref="Clear"/> instead.
    /// </remarks>
    public void Reset();
}

/// <summary>
/// A type of <see cref="IPrototypeConfig"/> for a <typeparamref name="TSubject"/> subject.
/// </summary>
public interface IPrototypeConfig<TSubject> : IPrototypeConfig
    where TSubject : class, IPrototypeSubject
{
    /// <summary>
    /// Applies this configuration to a blueprint <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The blueprint builder to which this configuration should be applied.</param>
    /// <returns>True if the configuration was applied, false if none was done/needed.</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public bool Apply(IBlueprintBuilder<TSubject> builder);
}
