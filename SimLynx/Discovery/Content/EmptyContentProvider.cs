namespace SimLynx.Discovery.Content;

/// <summary>
/// A content provider which does nothing and whose ID is <see cref="Symbol.Empty"/>.
/// </summary>
public class EmptyContentProvider : IContentProvider
{
    /// <summary>
    /// The static singleton instance.
    /// </summary>
    public static EmptyContentProvider Instance { get; } = new();

    /// <inheritdoc/>
    public Symbol Id => Symbol.Empty;
}
