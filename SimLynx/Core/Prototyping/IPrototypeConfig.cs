using System.ComponentModel;
using SimLynx.Core.Prototyping.Blueprints;

namespace SimLynx.Core.Prototyping;

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
    /// Clears this config, returning if clearing was successful. If this config had nothing to clear, <c>false</c> is
    /// returned.
    /// </summary>
    public bool Clear();
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
