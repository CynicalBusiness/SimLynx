
using System.Reflection;

namespace SimLynx.Core.Defs;

/// <summary>
/// Reflection information about a particular def property, used for configuring defs via "configuration" reflection.
/// </summary>
public interface IDefProperty
{

    /// <summary>
    /// The reflection information about the property this represents.
    /// </summary>
    public PropertyInfo Info { get; }

    /// <summary>
    /// Whether this property is required to be configured for the def to be valid.
    /// </summary>
    public bool IsRequired { get; }

}
