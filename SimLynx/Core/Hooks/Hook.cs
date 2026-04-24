using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SimLynx.Core.Hooks;

/// <summary>
/// Hooks provide a mechanism for listening to and responding to specific events or actions within the system, providing
/// IoC for various services and systems to interact without depending on each-other.
/// </summary>
/// <remarks>
/// This class is thread-safe for (un)subscription and invocation.
/// </remarks>
/// <typeparam name="TPayload">The type of payload the hook carries.</typeparam>
/// <param name="deliveryStrategy">The strategy to use for delivering hook invocations to subscribers.</param>
public class Hook<TPayload>(IHookDeliveryStrategy deliveryStrategy) : IDisposable
{
    private readonly SortedDictionary<sbyte, LinkedList<HookHandler<TPayload>>> _handlerMap = new(
        PriorityComparer.Default
    );
    private readonly ReaderWriterLockSlim _lock = new();

    /// <summary>
    /// Token to signal cancellation of this hook.
    /// </summary>
    protected readonly CancellationTokenSource cancellation = new();

    /// <summary>
    /// Dispatches an invocation containing the provided <paramref name="payload"/> on this hook to all subscribers,
    /// according to the hook's delivery strategy.
    /// </summary>
    /// <param name="payload">The payload to be delivered to subscribers.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the dispatch to complete.</param>
    /// <returns>A task that completes once all subscribers have completed.</returns>
    public virtual Task Invoke(TPayload payload, CancellationToken cancellationToken = default)
    {
        return Invoke(payload, PrepareContext(cancellationToken));
    }

    /// <summary>
    /// Dispatches an invocation containing the provided <paramref name="payload"/> on this hook to all subscribers,
    /// using the provided <paramref name="context"/>, according to the hook's delivery strategy.
    /// </summary>
    /// <param name="payload">The payload to be delivered to subscribers.</param>
    /// <param name="context">The context in which the hook is being invoked.</param>
    /// <returns>A task that completes once all subscribers have completed.</returns>
    public virtual async Task Invoke(TPayload payload, HookContext context)
    {
        // store the current handlers in an array so we can release the lock before actually invoking them.
        var handlerArrays = new HookHandler<TPayload>[_handlerMap.Count][];

        _lock.EnterReadLock();
        try
        {
            var i = 0;
            foreach (var handlers in _handlerMap.Values)
            {
                // the dictionary keeps this in priority order already
                var handlerArray = new HookHandler<TPayload>[handlers.Count];
                handlers.CopyTo(handlerArray, 0);
                handlerArrays[i++] = handlerArray;
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        // TODO investigate SynchronizationContext and TaskScheduler to better handle these invocations
        foreach (var handlerArray in handlerArrays)
        {
            await deliveryStrategy.Deliver(payload, context, handlerArray).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Subscribes a <paramref name="handler"/> to this hook with the specified <paramref name="priority"/>.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <param name="priority">The priority of the handler.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe the handler.</returns>
    public virtual IDisposable Subscribe(HookHandler<TPayload> handler, sbyte priority)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_handlerMap.TryGetValue(priority, out var handlers))
            {
                handlers = new LinkedList<HookHandler<TPayload>>();
                _handlerMap.Add(priority, handlers);
            }

            var node = handlers.AddLast(handler);
            return new DisposalAction(() =>
            {
                _lock.EnterWriteLock();
                try
                {
                    handlers.Remove(node);
                    if (handlers.Count == 0)
                    {
                        _handlerMap.Remove(priority);
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            });
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Subscribes a <paramref name="handler"/> to this hook with the default priority.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe the handler.</returns>
    public virtual IDisposable Subscribe(HookHandler<TPayload> handler)
    {
        return Subscribe(handler, Priorities.Default);
    }

    /// <summary>
    /// Subscribes the provided <paramref name="handler"/> to this hook with the handler's defined priority.
    /// </summary>
    /// <param name="handler">The handler to subscribe.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe the handler.</returns>
    public virtual IDisposable Subscribe(IHookHandler<TPayload> handler)
    {
        return Subscribe(handler.HandleHook, handler.Priority);
    }

    /// <summary>
    /// Prepares a new context for a hook invocation.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation of the invocation.</param>
    /// <returns>A new <see cref="HookContext"/> instance.</returns>
    protected virtual HookContext PrepareContext(CancellationToken cancellationToken)
    {
        var c = CancellationTokenSource.CreateLinkedTokenSource(cancellation.Token, cancellationToken);
        return new HookContext { DispatchedAt = DateTime.UtcNow, CancellationToken = c.Token };
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        cancellation.Cancel();
        cancellation.Dispose();
    }
}
