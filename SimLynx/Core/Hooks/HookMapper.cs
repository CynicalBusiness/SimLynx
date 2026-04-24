using System;
using System.Threading.Tasks;

namespace SimLynx.Core.Hooks;

/// <summary>
/// A hook handler which delegates handling of a hook to another hook, mapping the payload from one type to another in
/// the process.
/// </summary>
/// <typeparam name="TSource">The type of the source payload.</typeparam>
/// <typeparam name="TTarget">The type of the target payload.</typeparam>
/// <param name="mapFunc">The function used to map the source payload to the target payload.</param>
/// <param name="targetHook">The target hook to which the mapped payload will be delegated.</param>
/// <param name="priority">The priority of the hook handler.</param>
public class HookMapper<TSource, TTarget>(
    HookMapper<TSource, TTarget>.MapFunc mapFunc,
    Hook<TTarget> targetHook,
    sbyte priority = Priorities.VeryLow
) : IHookHandler<TSource>
{
    /// <summary>
    /// Delegate which maps a source payload to a target payload for delegation to another hook.
    /// </summary>
    /// <param name="source">The source payload to be mapped.</param>
    /// <returns>The mapped target payload.</returns>
    public delegate TTarget MapFunc(TSource source);

    /// <inheritdoc/>
    public sbyte Priority { get; } = priority;

    /// <inheritdoc/>
    public Task HandleHook(TSource payload, HookContext context)
    {
        var targetPayload = mapFunc(payload);
        return targetHook.Invoke(targetPayload, context);
    }
}
