using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SimLynx;

/// <summary>
/// Helper that allows storing any arbitrary metadata on any managed object, without holding a strong reference to it.
/// </summary>
/// <remarks>
/// Objects with metadata can be garbage collected as usual, and the metadata, too, will be collected afterward,
/// if applicable.
/// </remarks>
public static class Metadata
{
    private static readonly ConditionalWeakTable<object, ITypeDictionary<object>> metadataTable = new();

    /// <summary>
    /// Gets the metadata dictionary for the given object. If no metadata exists yet, a new dictionary is created and
    /// associated with the object.
    /// </summary>
    /// <param name="obj">The object to get metadata for.</param>
    /// <returns>The metadata dictionary associated with the object.</returns>
    public static ITypeDictionary<object> Get(object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return metadataTable.GetValue(obj, _ => new TypeDictionary<object>());
    }

    /// <summary>
    /// Tries to get the metadata dictionary for the given object. If no metadata exists yet, returns <c>false</c>.
    /// </summary>
    /// <param name="obj">The object to get metadata for.</param>
    /// <param name="metadata">The metadata dictionary associated with the object, if it exists.</param>
    /// <returns><c>true</c> if the metadata dictionary exists; otherwise, <c>false</c>.</returns>
    public static bool TryGet(object obj, [NotNullWhen(true)] out ITypeDictionary<object>? metadata)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return metadataTable.TryGetValue(obj, out metadata);
    }
}
