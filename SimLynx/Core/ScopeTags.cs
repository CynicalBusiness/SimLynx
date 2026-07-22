using System;
using System.Linq;

namespace SimLynx.Core;

/// <summary>
/// Helper class to tag Autofac lifetime scopes with multiple tags.
/// </summary>
public class ScopeTags(params object[] tags) : IEquatable<ScopeTags>
{
    /// <summary>
    /// Checks if the given object is one of the tags for this scope.
    /// </summary>
    /// <remarks>
    /// This allows for lifetime scopes to be tagged with multiple tags, and for registrations to specify any of those tags
    /// as their lifetime scope tag.
    /// </remarks>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if the object is one of the tags; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        // ? Autofac uses ".Contains" to check scope matches, which should invoke this method
        return base.Equals(obj) || Tags.Contains(obj);
    }

    /// <inheritdoc/>
    public bool Equals(ScopeTags? other)
    {
        return other is not null && Tags.SequenceEqual(other.Tags);
    }

    private object[] Tags { get; } = tags;

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Tags.GetHashCode();
    }
}
