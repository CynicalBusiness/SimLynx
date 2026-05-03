using System;
using System.ComponentModel;

namespace SimLynx.Core;

/// <summary>
/// State representing the lifecycle of an "operational" object. That is, an object which can be started and stopped,
/// optionally with a notion of being "scheduled" to do so.
/// </summary>
/// <remarks>
/// The exact meaning of each state may depend on the context of the object providing it.
/// </remarks>
[Flags]
public enum OperationalState
{
    // * flags

    /// <summary>
    /// Indicates the object is requested to be operational.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    IsRequested = 1 << 0,

    /// <summary>
    /// Indicates the object has finished its operation and potentially has a result available, if applicable.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    IsRealized = 1 << 1,

    /// <summary>
    /// Indicates the object is in a faulted state, such as from an unhandled exception during execution.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    IsFaulted = 1 << 2,

    // * defined base states for each flag

    /// <summary>
    /// Object is scheduled/requested to be operational, but isn't yet.
    /// </summary>
    Pending = IsRequested,

    /// <summary>
    /// Object has finished its operation without issue.
    /// </summary>
    Finished = IsRealized,

    /// <summary>
    /// Object is in an invalid state that prevents it from operating and will reject requests to do so.
    /// </summary>
    Invalid = IsFaulted,

    // * three "combination" states for each combination of the above flags.

    /// <summary>
    /// Object is actively operating and hasn't yet stopped.
    /// </summary>
    Active = IsRequested | IsRealized,

    /// <summary>
    /// Object has finished its operation, but unsuccessfully.
    /// </summary>
    Failed = IsRealized | IsFaulted,

    /// <summary>
    /// Object was requested to operate, but is unable to do so.
    /// </summary>
    Suspended = IsRequested | IsFaulted,

    // * min/max states

    /// <summary>
    /// Object is not currently operational nor requested to be.
    /// </summary>
    Inactive = 0,

    /// <summary>
    /// Object is in an unknown state.
    /// </summary>
    Unknown = IsRequested | IsRealized | IsFaulted,
}
