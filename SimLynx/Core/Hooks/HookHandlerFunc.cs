using System.Threading.Tasks;

namespace SimLynx.Core.Hooks;

/// <summary>
/// Handler delegate for <typeparamref name="TPayload"/> hooks which subscribers should implement.
/// </summary>
/// <typeparam name="TPayload">The type of payload.</typeparam>
/// <param name="payload">The payload to be processed by the hook.</param>
/// <param name="context">The context in which the hook is being executed.</param>
/// <returns>A task representing the operation.</returns>
public delegate Task HookHandlerFunc<in TPayload>(TPayload payload, HookContext context);
