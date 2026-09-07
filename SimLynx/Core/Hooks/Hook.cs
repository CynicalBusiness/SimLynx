using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SimLynx.Core.Hooks;

/// <summary>
/// Hooks provide a mechanism for listening to and responding to specific events or actions within the system, providing
/// IoC for various services and systems to interact without depending on each-other.
/// </summary>
/// <remarks>
/// This class is thread-safe for (un)subscription and invocation.
/// </remarks>
/// <typeparam name="TPayload">The type of payload the hook carries.</typeparam>
public partial class Hook<TPayload> : IHook<TPayload>, IDisposable
{
    private readonly SortedDictionary<sbyte, LinkedList<IHookHandler<TPayload>>> _handlerMap = new(
        PriorityComparer.Default
    );
    private readonly ReaderWriterLockSlim _lock = new();

    /// <summary>
    /// Event raised before invoking handlers for a hook invocation. Handlers subscribed to this event will be invoked
    /// before any handlers subscribed to the hook itself, and will not be affected by the hook delivery strategy.
    /// </summary>
    public event Action<TPayload, HookContext>? OnBeforeInvoke;

    /// <summary>
    /// Event raised after invoking handlers for a hook invocation. Handlers subscribed to this event will be invoked
    /// after all handlers subscribed to the hook itself have completed, and will not be affected by the hook delivery
    /// strategy.
    /// </summary>
    public event Action<TPayload, HookContext>? OnAfterInvoke;

    /// <summary>
    /// Event raised if an exception is thrown during the invocation of any handler for a hook invocation. Handlers
    /// subscribed to this event will be invoked immediately when an exception is thrown, and will receive the
    /// exception as a parameter. They will not be affected by the hook delivery strategy.
    /// </summary>
    public event Action<TPayload, HookContext, Exception>? OnInvokeError;

    /// <summary>
    /// The strategy to use for delivering hook invocations to subscribers.
    /// </summary>
    protected readonly IHookDeliveryStrategy deliveryStrategy;

    private readonly ILogger<Hook<TPayload>> logger;

    /// <summary>
    /// Creates a new hook with the given delivery strategy and initial handlers.
    /// </summary>
    /// <param name="deliveryStrategy">The strategy to use for delivering hook invocations to subscribers.</param>
    /// <param name="handlers">The initial handlers to subscribe to this hook.</param>
    /// <param name="logger">The logger instance.</param>
    public Hook(
        IHookDeliveryStrategy deliveryStrategy,
        IEnumerable<IHookHandler<TPayload>>? handlers = null,
        ILogger<Hook<TPayload>>? logger = null
    )
    {
        ArgumentNullException.ThrowIfNull(deliveryStrategy);
        this.deliveryStrategy = deliveryStrategy;

        if (handlers is not null)
        {
            foreach (var handler in handlers)
            {
                Subscribe(handler);
            }
        }

        this.logger = logger ?? NullLogger<Hook<TPayload>>.Instance;
    }

    /// <summary>
    /// Token to signal cancellation of this hook.
    /// </summary>
    protected readonly CancellationTokenSource cancellation = new();

    /// <inheritdoc/>
    public virtual Task Invoke(TPayload payload, CancellationToken cancellationToken = default)
    {
        return Invoke(payload, PrepareContext(cancellationToken));
    }

    /// <inheritdoc/>
    public virtual async Task Invoke(TPayload payload, HookContext context)
    {
        OnBeforeInvoke?.Invoke(payload, context);

        // store the current handlers in an array so we can release the lock before actually invoking them.
        var handlerMap = new Dictionary<sbyte, IReadOnlyCollection<IHookHandler<TPayload>>>();

        _lock.EnterReadLock();
        try
        {
            foreach (var (priority, handlers) in _handlerMap)
            {
                // the dictionary keeps this in priority order already
                var handlerArray = new IHookHandler<TPayload>[handlers.Count];
                handlers.CopyTo(handlerArray, 0);
                handlerMap[priority] = handlerArray;
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        LogEvent.HookInvoking(logger, nameof(Hook<>), typeof(TPayload));
        var stopwatch = Stopwatch.StartNew();

        // TODO investigate SynchronizationContext and TaskScheduler to better handle these invocations
        try
        {
            await deliveryStrategy.Deliver(payload, context, handlerMap).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogEvent.HookInvokeError(logger, nameof(Hook<>), typeof(TPayload), stopwatch.Elapsed, ex);
            OnInvokeError?.Invoke(payload, context, ex);
            throw;
        }

        stopwatch.Stop();
        LogEvent.HookInvoked(logger, nameof(Hook<>), typeof(TPayload), stopwatch.Elapsed);
        OnAfterInvoke?.Invoke(payload, context);
    }

    /// <inheritdoc/>
    public virtual IDisposable Subscribe(IHookHandler<TPayload> handler)
    {
        var priority = handler.Priority;
        _lock.EnterWriteLock();
        try
        {
            if (!_handlerMap.TryGetValue(priority, out var handlers))
            {
                handlers = new LinkedList<IHookHandler<TPayload>>();
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

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Hook<{typeof(TPayload)}>";
    }

    /// <summary>
    /// <see cref="Hook{TPayload}"/> log events.
    /// </summary>
    public static partial class LogEvent
    {
        /// <summary>
        /// Event ID for a hook being invoked.
        /// </summary>
        public const int HOOK_INVOKING = 1;

        [LoggerMessage(
            EventId = HOOK_INVOKING,
            Level = LogLevel.Debug,
            Message = "Invoking hook: {hookName}<{payloadType}>"
        )]
        internal static partial void HookInvoking(ILogger logger, string hookName, Type payloadType);

        /// <summary>
        /// Event ID for a hook having been invoked.
        /// </summary>
        public const int HOOK_INVOKED = 2;

        [LoggerMessage(
            EventId = HOOK_INVOKED,
            Level = LogLevel.Debug,
            Message = "Hook invoked: {hookName}<{payloadType}>, took {elapsed}"
        )]
        internal static partial void HookInvoked(ILogger logger, string hookName, Type payloadType, TimeSpan elapsed);

        /// <summary>
        /// Event ID for an error occurring during hook invocation.
        /// </summary>
        public const int HOOK_INVOKE_ERROR = 3;

        [LoggerMessage(
            EventId = HOOK_INVOKE_ERROR,
            Level = LogLevel.Error,
            Message = "Exception in hook: {hookName}<{payloadType}> (after {elapsed})"
        )]
        internal static partial void HookInvokeError(
            ILogger logger,
            string hookName,
            Type payloadType,
            TimeSpan elapsed,
            Exception exception
        );
    }
}
