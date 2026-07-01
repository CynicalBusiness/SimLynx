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
    /// The resolver of this configuration, which is responsible for creating and managing it.
    /// </summary>
    public IPrototypeConfigResolver Resolver { get; }

    /// <summary>
    /// Clears this config, returning if clearing was successful. If this config had nothing to clear, <c>false</c> is
    /// returned.
    /// </summary>
    public bool Clear();
}
