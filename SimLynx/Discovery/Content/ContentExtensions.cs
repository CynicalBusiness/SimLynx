using Semver;

namespace SimLynx.Discovery.Content;

/// <summary>
/// Extension methods for content-related types.
/// </summary>
public static class ContentExtensions
{
    extension(IContentPackage pkg)
    {
        /// <summary>
        /// Gets the fully-qualified identifier of the content package.
        /// </summary>
        public Symbol Id => pkg.Manifest.Id;

        /// <summary>
        /// Gets the version of the content package.
        /// </summary>
        public SemVersion Version => pkg.Manifest.Version;
    }
}
