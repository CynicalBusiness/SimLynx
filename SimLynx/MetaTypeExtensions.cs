using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace SimLynx;

/// <summary>
/// Extensions for working with common data on <see cref="IMetaType"/> instances.
/// </summary>
public static class MetaTypeExtensions
{
    /// <summary>
    /// A tag used to cache the properties on a meta-type.
    /// </summary>
    public static readonly Symbol PropertiesTag = Symbol.For(nameof(get_Properties));

    /// <summary>
    /// A tag used to cache the properties on a meta-type, keyed by property name.
    /// </summary>
    public static readonly Symbol PropertiesByNameTag = Symbol.For(nameof(get_PropertiesByName));

    extension(IMetaType @this)
    {
        /// <summary>
        /// A cached array of all properties on the underlying type.
        /// </summary>
        public PropertyInfo[] Properties => @this.Metadata.GetOrAdd(PropertiesTag, () => @this.Type.GetProperties());

        /// <summary>
        /// Gets a cached dictionary of all properties on the underlying type, keyed by property name.
        /// </summary>
        /// <returns>The dictionary of properties</returns>
        public IReadOnlyDictionary<string, PropertyInfo> PropertiesByName =>
            @this.Metadata.GetOrAdd<IReadOnlyDictionary<string, PropertyInfo>>(
                PropertiesByNameTag,
                () => new ReadOnlyDictionary<string, PropertyInfo>(@this.Type.GetProperties().ToDictionary(p => p.Name))
            );

        /// <summary>
        /// Tries to get a property by name from the underlying type.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <param name="propertyInfo">The property info if found; otherwise, null.</param>
        /// <returns>True if the property was found; otherwise, false.</returns>
        public bool TryGetProperty(string propertyName, [MaybeNullWhen(false)] out PropertyInfo propertyInfo)
        {
            return @this.PropertiesByName.TryGetValue(propertyName, out propertyInfo);
        }

        /// <summary>
        /// Gets a property by name from the underlying type.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>W
        /// <returns>The property info if found; otherwise, null.</returns>
        public PropertyInfo? GetProperty(string propertyName)
        {
            return @this.PropertiesByName.GetValueOrDefault(propertyName);
        }
    }
}
