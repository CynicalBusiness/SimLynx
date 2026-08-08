using Semver;

namespace SimLynx.Discovery.Content;

/// <summary>
/// Package of content.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ContentPackage"/> class with the specified manifest.
/// </remarks>
/// <param name="manifest">The manifest that describes this content package.</param>
public abstract class ContentPackage(ContentPackageManifest manifest) : IContentProvider
{
    /// <summary>
    /// The tag that identifies a prerelease content package.
    /// </summary>
    public static readonly Symbol ContentPrereleaseTag = Symbol.For("prerelease");

    /// <summary>
    /// The manifest that describes this package.
    /// </summary>
    public ContentPackageManifest Manifest { get; } = manifest;

    /// <inheritdoc/>
    public Symbol Id => Manifest.Id;

    /// <summary>
    /// The version of the package.
    /// </summary>
    public SemVersion Version => Manifest.Version;
}
