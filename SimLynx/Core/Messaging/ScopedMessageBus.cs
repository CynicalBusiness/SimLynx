using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace SimLynx.Core.Messaging;

internal class ScopedMessageBus(ParentLifetimeScopeAccessor parentLifetimeScopeAccessor) : IMessageBus, IDisposable
{
    private static readonly ConcurrentDictionary<Type, ICollection<Type>> targetTypeCache = [];

    public static ICollection<Type> GetTargetMessageTypes(Type messageType)
    {
        return targetTypeCache.GetOrAdd(messageType, CreateTargetTypeCollection);
    }

    private static ICollection<Type> CreateTargetTypeCollection(Type messageType)
    {
        return [messageType, .. messageType.GetBaseTypes(), .. messageType.GetInterfaces()];
    }

    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<Type, IMessageSubscriptionChannel> _channels = [];
    private readonly List<IDisposable> _parentSubscriptions = [];
    private readonly CancellationTokenSource _cts = new();

    public IDisposable Subscribe<TPayload>(MessageHandler<TPayload> messageHandler, sbyte priority)
    {
        _lock.EnterWriteLock();
        try
        {
            MessageSubscriptionChannel<TPayload> channel;
            if (!_channels.TryGetValue(typeof(TPayload), out var c))
            {
                c = channel = new MessageSubscriptionChannel<TPayload>();
                _channels.Add(typeof(TPayload), c);

                // if there's a parent bus available, subscribe to it so messages propagate downward
                var parentBus = parentLifetimeScopeAccessor.ParentScope?.ResolveOptional<IMessageBus>();
                if (parentBus is not null && parentBus != this)
                {
                    _parentSubscriptions.Add(
                        parentBus.Subscribe<TPayload>(
                            (message, token) => Dispatch(message, token),
                            Priorities.VeryLow + Priorities.Low
                        )
                    );
                }
            }
            else
            {
                channel = (MessageSubscriptionChannel<TPayload>)c;
            }
            channel.Add(messageHandler, priority);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return new DisposalAction(() => Unsubscribe(messageHandler, priority));
    }

    public bool Unsubscribe<TPayload>(MessageHandler<TPayload> messageHandler, sbyte priority)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_channels.TryGetValue(typeof(TPayload), out var c))
            {
                var channel = (MessageSubscriptionChannel<TPayload>)c;
                return channel.Remove(messageHandler, priority);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return false;
    }

    public Task<IMessage<TPayload>> Send<TPayload>(TPayload payload, CancellationToken cancellationToken = default)
    {
        return Dispatch(new Message<TPayload>(payload), cancellationToken);
    }

    public async Task<IMessage<TPayload>> Dispatch<TPayload>(
        IMessage<TPayload> message,
        CancellationToken cancellationToken
    )
    {
        cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token).Token;
        cancellationToken.ThrowIfCancellationRequested();

        // pull the relevant handlers outside of the lock to avoid holding it during handler execution
        var targetTypes = GetTargetMessageTypes(typeof(TPayload));
        var channels = new List<IMessageSubscriptionChannel>(targetTypes.Count);

        _lock.EnterReadLock();
        try
        {
            foreach (var targetType in targetTypes)
            {
                if (_channels.TryGetValue(targetType, out var c))
                {
                    channels.Add(c);
                }
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        foreach (var task in channels.SelectMany(i => i.Invoke(message, cancellationToken)))
        {
            // TODO set up some parallelism in the future
            // just series for now to keep things simple
            await task;
        }
        return message;
    }

    public void Dispose()
    {
        _lock.EnterWriteLock();
        try
        {
            foreach (var channel in _channels.Values)
            {
                channel.Dispose();
            }
            _channels.Clear();

            foreach (var subscription in _parentSubscriptions)
            {
                subscription.Dispose();
            }
            _parentSubscriptions.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
        _cts.Cancel();
        _lock.Dispose();
    }

    private interface IMessageSubscriptionChannel : IDisposable
    {
        public IEnumerable<Task> Invoke(IMessage message, CancellationToken cancellationToken);
    }

    private class MessageSubscriptionChannel<TPayload> : IMessageSubscriptionChannel
    {
        private readonly SortedDictionary<sbyte, LinkedSet<MessageHandler<TPayload>>> _handlerSets = new(
            PriorityComparer.Default
        );
        public readonly ResetLazy<MessageHandler<TPayload>[][]> Handlers;

        public MessageSubscriptionChannel()
        {
            Handlers = new(GetHandlers);
        }

        public void Add(MessageHandler<TPayload> handler, sbyte priority)
        {
            if (!_handlerSets.TryGetValue(priority, out var handlerSet))
            {
                _handlerSets.Add(priority, handlerSet = []);
            }
            handlerSet.Add(handler);
            Handlers.Reset();
        }

        public bool Remove(MessageHandler<TPayload> handler, sbyte priority)
        {
            if (_handlerSets.TryGetValue(priority, out var handlerSet))
            {
                if (handlerSet.Remove(handler))
                {
                    if (handlerSet.Count == 0)
                    {
                        _handlerSets.Remove(priority);
                    }
                    Handlers.Reset();
                    return true;
                }
            }
            return false;
        }

        private MessageHandler<TPayload>[][] GetHandlers()
        {
            var allHandlers = new MessageHandler<TPayload>[_handlerSets.Count][];

            var i = 0;
            foreach (var handlerSet in _handlerSets.Values)
            {
                var handlers = new MessageHandler<TPayload>[handlerSet.Count];
                handlerSet.CopyTo(handlers, 0);
                allHandlers[i++] = handlers;
            }

            return allHandlers;
        }

        public void Dispose()
        {
            _handlerSets.Clear();
        }

        public IEnumerable<Task> Invoke(IMessage message, CancellationToken cancellationToken)
        {
            var handlers = Handlers.Value;
            foreach (var handler in handlers.SelectMany(i => i))
            {
                yield return handler.Invoke((IMessage<TPayload>)message, cancellationToken);
            }
        }
    }
}
