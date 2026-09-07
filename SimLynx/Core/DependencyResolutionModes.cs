using System;

namespace SimLynx.Core;

/// <summary>
/// Flags that describe how a dependency should be resolved.
/// </summary>
[Flags]
public enum DependencyResolutionModes
{
    /// <summary>
    /// Default optional dependency behavior.
    /// </summary>
    Optional = 0,

    /// <summary>
    /// Indicates the dependency is required and must be satisfied.
    /// </summary>
    /// <remarks>
    /// Depending on resolvers, a "just-in-time" JIT resolution may be able to satisfy a dependency if not otherwise
    /// available. This flag indicates that should even that fail (or not be possible), an error should be raised.
    /// </remarks>
    Required = 1,

    /// <summary>
    /// Indicates the dependency must be satisfied exactly, not by a derived type or range.
    /// </summary>
    /// <remarks>
    /// Depending on what is being resolved, dependencies could possibly be satisfied by a derived type or a compatible
    /// selection from a range of values. This flag disables this behavior and required an exact match.
    /// </remarks>
    Exact = 1 << 2,

    /// <summary>
    /// Indicates the dependency must be satisfied explicitly, even when JIT resolution could otherwise satisfy it.
    /// </summary>
    /// <remarks>
    /// This flag can be used to indicate a "peer" dependency which can be satisfied only if it is explicitly provided
    /// or requested by something else.
    /// </remarks>
    Explicit = 1 << 3,
}
