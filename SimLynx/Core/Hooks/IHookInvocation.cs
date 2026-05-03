namespace SimLynx.Core.Hooks;

/// <summary>
/// An invocation of a hook.
/// </summary>
public interface IHookInvocation
{
    /// <summary>
    /// Context of the invocation.
    /// </summary>
    public HookContext Context { get; }

    /// <summary>
    /// Payload for the invocation, if any.
    /// </summary>
    public object? Payload { get; }
}

/// <summary>
/// An invocation of a <typeparamref name="TPayload"/> hook.
/// </summary>
/// <typeparam name="TPayload">The type of the payload for the invocation.</typeparam>
public interface IHookInvocation<out TPayload> : IHookInvocation
{
    /// <summary>
    /// Payload for the invocation
    /// </summary>
    public new TPayload Payload { get; }

    object? IHookInvocation.Payload => Payload;
}
