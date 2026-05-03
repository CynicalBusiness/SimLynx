using System.Threading.Tasks;

namespace SimLynx.Core.Hooks;

/// <summary>
/// A simple implementation of <see cref="IHookHandler{TPayload}"/> which wraps a
/// <see cref="HookHandlerFunc{TPayload}"/> delegate.
/// </summary>
/// <typeparam name="TPayload">The type of the payload.</typeparam>
/// <param name="HandlerFunc">The handler function.</param>
/// <param name="Source">The source of the hook.</param>
/// <param name="Priority">The priority of the hook.</param>
public record class HookHandler<TPayload>(
    HookHandlerFunc<TPayload> HandlerFunc,
    object? Source = null,
    sbyte Priority = Priorities.Default
) : IHookHandler<TPayload>
{
    /// <inheritdoc/>
    public Task HandleHook(TPayload payload, HookContext context)
    {
        return HandlerFunc.Invoke(payload, context);
    }

    object? IHookHandler.Source => Source;
}
