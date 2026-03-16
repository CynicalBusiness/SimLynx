
namespace SimLynx.Discovery.Content;

/// <summary>
/// Generalized interface for packages of content.
/// </summary>
public interface IContentPackage
{

    /// <summary>
    /// The manifest that describes this package.
    /// </summary>
    public ContentPackageManifest Manifest { get; }

}
