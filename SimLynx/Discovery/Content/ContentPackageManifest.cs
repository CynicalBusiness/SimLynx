
using Semver;

namespace SimLynx.Discovery.Content;

/// <summary>
/// A manifest that describes content available in a particular location/package.
/// <br/>
/// This manifest can be used to read information about potential content packages without having to load them.
/// </summary>
public record ContentPackageManifest
{

    /// <summary>
    /// The fully-qualified identifier of the content package.
    /// <br/>
    /// In general, this should be a .NET-style namespace, i.e. "MyCompany.MyGame"
    /// </summary>
    public required Symbol Id { get; init; }

    /// <summary>
    /// The version of the content package.
    /// </summary>
    public required SemVersion Version { get; init; }

}
