namespace SimLynx.Discovery.Content;

/// <summary>
/// A generic object which can provide content to SimLynx.
/// </summary>
public interface IContentProvider
{
    /// <summary>
    /// The provider's unique identifier.
    /// </summary>
    public Symbol Id { get; }
}
