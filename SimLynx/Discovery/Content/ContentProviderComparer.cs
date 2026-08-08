using System.Collections.Generic;

namespace SimLynx.Discovery.Content;

/// <summary>
/// Provides a comparer for <see cref="IContentProvider"/> instances based on their
/// <see cref="IContentProvider.Id"/> property.
/// </summary>
public class ContentProviderComparer : IEqualityComparer<IContentProvider>
{
    /// <summary>
    /// The default instance.
    /// </summary>
    public static ContentProviderComparer Default { get; } = new();

    private ContentProviderComparer() { }

    /// <inheritdoc/>
    public bool Equals(IContentProvider? x, IContentProvider? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null || y is null)
            return false;
        return x.Id == y.Id;
    }

    /// <inheritdoc/>
    public int GetHashCode(IContentProvider obj)
    {
        return obj.Id.GetHashCode();
    }
}
