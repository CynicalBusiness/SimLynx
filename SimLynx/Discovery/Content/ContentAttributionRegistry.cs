using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SimLynx.Discovery.Content;

/// <summary>
/// Represents a registry for attributing activities to their respective content provider.
/// </summary>
/// <param name="defaultProvider">The provider to use when no specific provider is found.</param>
public class ContentAttributionRegistry(IContentProvider defaultProvider)
{
    private static readonly ContentAttributionRegistry _default = new(EmptyContentProvider.Instance);
    private static readonly AsyncLocal<ContentAttributionRegistry> _current = new();

    /// <summary>
    /// The current registry available in context.
    /// </summary>
    public static ContentAttributionRegistry Current
    {
        get => _current.Value ?? _default;
        internal set => _current.Value = value;
    }

    /// <summary>
    /// The default content provider to use when no specific provider is found.
    /// </summary>
    public IContentProvider DefaultProvider { get; } = defaultProvider;

    /// <summary>
    /// Attempts to get the content provider associated with the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly for which to get the content provider.</param>
    /// <param name="provider">The content provider associated with the specified assembly, if found.</param>
    /// <returns>True if a content provider was found for the specified assembly; otherwise, false.</returns>
    public bool TryGetForAssembly(Assembly assembly, out IContentProvider provider)
    {
        // TODO Implement logic to determine the content provider for the given assembly.
        // For now, return the default provider.
        provider = DefaultProvider;
        return false;
    }

    /// <summary>
    /// Gets the content provider associated with the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly for which to get the content provider.</param>
    /// <returns>The content provider associated with the specified assembly.</returns>
    public IContentProvider GetForAssembly(Assembly assembly)
    {
        return TryGetForAssembly(assembly, out var provider) ? provider : DefaultProvider;
    }

    /// <summary>
    /// Attributes the current caller to a content provider based on its all calling assemblies in the current
    /// stack, returning the first provider found that is not the default provider, or the default provider if no other
    /// provider is found.
    /// </summary>
    /// <returns>The content provider associated with the current caller.</returns>
    public IContentProvider GetForCaller()
    {
        return Assembly
                .GetCallingAssemblies()
                .TrySelect(
                    (Assembly assembly, [MaybeNullWhen(false)] out IContentProvider provider) =>
                        TryGetForAssembly(assembly, out provider)
                )
                .FirstOrDefault(provider => provider != DefaultProvider)
            ?? DefaultProvider;
    }
}
