using Autofac.Core;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Lifetime scope for a specific phase ID.
/// </summary>
/// <param name="phaseId">The ID of the phase.</param>
public class PhaseIdScopeLifetime(string phaseId) : IComponentLifetime
{
    /// <summary>
    /// The ID of the phase that this lifetime is associated with.
    /// </summary>
    public string PhaseId { get; } = phaseId;

    /// <inheritdoc/>
    public ISharingLifetimeScope FindScope(ISharingLifetimeScope mostNestedVisibleScope)
    {
        var current = mostNestedVisibleScope;
        while (current is not null)
        {
            if (current.Tag is PhaseScopeTag tag && tag.PhaseId == PhaseId)
            {
                return current;
            }
            current = current.ParentLifetimeScope;
        }

        throw new DependencyResolutionException($"No lifetime scope found for phase ID: {PhaseId}");
    }
}
