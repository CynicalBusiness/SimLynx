namespace SimLynx.Core.Hooks;

/// <summary>
/// Marker interface for hooks.
/// </summary>
public interface IHook { }

/// <summary>
/// Generic version of <see cref="IHook"/> for hooks that carry a payload of type <typeparamref name="TPayload"/>.
/// </summary>
/// <typeparam name="TPayload">The type of the payload for the hook.</typeparam>
public interface IHook<TPayload> : IHook, IHookable<TPayload>, IHookInvocable<TPayload> { }
