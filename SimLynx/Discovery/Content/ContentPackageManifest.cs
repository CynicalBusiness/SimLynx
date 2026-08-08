using System;
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
    /// Creates a new content package manifest with information inferred from the specified content package type.
    /// </summary>
    /// <typeparam name="TPackage">The type of the content package.</typeparam>
    /// <returns>A new content package manifest.</returns>
    public static ContentPackageManifest From<TPackage>()
        where TPackage : ContentPackage
    {
        var packageType = typeof(TPackage);

        var id = Symbol.For(packageType.FullName!);
        var name = packageType.Name;
        var version = SemVersion.FromVersion(packageType.Assembly.GetName().Version ?? new Version(0, 1, 0));

        return new ContentPackageManifest()
        {
            Id = id,
            Name = name,
            Version = version,
        };
    }

    /// <summary>
    /// The fully-qualified identifier of the content package.
    /// </summary>
    /// <remarks>
    /// In general, this should be a .NET-style namespace, i.e. `MyCompany.MyGame`.
    /// </remarks>
    public required Symbol Id { get; init; }

    /// <summary>
    /// The human-readable name of the content package.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The version of the content package.
    /// </summary>
    public required SemVersion Version { get; init; }
}
