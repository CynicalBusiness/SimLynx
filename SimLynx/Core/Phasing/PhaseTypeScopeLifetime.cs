using System;
using Autofac.Core;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Lifetime scope for a specific phase type.
/// </summary>
/// <param name="phaseType">The type of the phase.</param>
public class PhaseTypeScopeLifetime(Type phaseType) : IComponentLifetime
{
    /// <summary>
    /// The type of the phase that this lifetime is associated with.
    /// </summary>
    public Type PhaseType { get; } = phaseType;

    /// <inheritdoc/>
    public ISharingLifetimeScope FindScope(ISharingLifetimeScope mostNestedVisibleScope)
    {
        var current = mostNestedVisibleScope;
        while (current is not null)
        {
            if (current.Tag is PhaseScopeTag tag && tag.PhaseType == PhaseType)
            {
                return current;
            }
            current = current.ParentLifetimeScope;
        }

        throw new DependencyResolutionException($"No lifetime scope found for phase type: {PhaseType}");
    }
}
