using System.Threading.Tasks;

namespace SimLynx.Core.Hooks;

/// <summary>
/// Marker interface for classes which can handle hooks.
/// </summary>
/// <remarks>
/// Don't use this interface directly; instead, use the generic
/// <see cref="IHookHandler{TPayload}"/>.
/// <br/>
/// Used mostly for reflection and discovery.
/// </remarks>
public interface IHookHandler { }

/// <summary>
/// Interface for classes which can handle <typeparamref name="TPayload"/> hooks.
/// </summary>
/// <typeparam name="TPayload">The type hook payload.</typeparam>
public interface IHookHandler<in TPayload> : IHookHandler
{
    /// <summary>
    /// The priority of this handler.
    /// </summary>
    public sbyte Priority => Priorities.Default;

    /// <summary>
    /// Handles a hook invocation with the given payload and context.
    /// </summary>
    /// <param name="payload">The payload of the hook.</param>
    /// <param name="context">The context of the hook.</param>
    /// <returns>A task representing the operation.</returns>
    public Task HandleHook(TPayload payload, HookContext context);
}
