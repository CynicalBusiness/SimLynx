using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using SimLynx.Core.Logging;

namespace SimLynx.Core.Messaging;

/// <summary>
/// <see cref="IMessageBus"/> implementation which is scoped to a lifetime scope, and automatically subscribes all
/// <see cref="IMessageSubscriber{T}"/> registered within that scope.
/// </summary>
public class ScopedMessageBus : IMessageBus, IMessageSubscriber<IMessage>
{
    /// <summary>
    /// Event ID for message emissions, used for logging.
    /// </summary>
    public static readonly LogEvent<string> MessageEmittedLogEvent = new(
        EventId.For<ScopedMessageBus>("MessageEmitted"),
        "Message emitted: {MessageType}",
        LogLevel.Trace
    );

    private static IEnumerable<(IMessageSubscriber Subscriber, Type MessageType)> GetRegisteredSubscribers(
        IComponentContext ctx
    )
    {
        // get all registrations which are *registered* as closed types of IMessageSubscriber<>
        // we don't want to resolve anything that might be assignable but isn't registered as such
        foreach (IComponentRegistration reg in ctx.ComponentRegistry.Registrations)
        {
            if (!reg.Activator.LimitType.IsAssignableTo(typeof(IMessageSubscriber)))
            {
                continue;
            }

            foreach (var service in reg.Services.OfType<TypedService>())
            {
                if (
                    !service.ServiceType.IsGenericType
                    || service.ServiceType.GetGenericTypeDefinition() != typeof(IMessageSubscriber<>)
                )
                {
                    continue;
                }

                var messageType = service.ServiceType.GetGenericArguments()[0];
                var subscriber = (IMessageSubscriber)
                    ctx.ResolveComponent(new ResolveRequest(service, new(reg.ResolvePipeline, reg), []));
                yield return (subscriber, messageType);
            }
        }
    }

    private readonly ConcurrentDictionary<Type, SubscriptionSet> _subscriptions = [];
    private readonly ILogger<ScopedMessageBus> logger;

    /// <inheritdoc/>
    public sbyte Priority { get; } = MessageSubscriberPriority.LOWEST;

    internal ScopedMessageBus(
        ParentLifetimeScopeAccessor parentLifetimeScopeAccessor,
        IComponentContext ctx,
        ILogger<ScopedMessageBus> logger
    )
    {
        this.logger = logger;
        parentLifetimeScopeAccessor.ParentScope?.Resolve<IMessageBus>().Subscribe(this);

        foreach (var (subscriber, messageType) in GetRegisteredSubscribers(ctx))
        {
            Subscribe(messageType, subscriber);
        }
    }

    /// <inheritdoc/>
    public IMessageBus Emit<TMessage>(in TMessage message)
        where TMessage : IMessage
    {
        var messageType = message.GetType();
        MessageEmittedLogEvent.Log(logger, messageType.ToString());
        foreach (var emissionType in GetEmissionTypes(messageType))
        {
            if (_subscriptions.TryGetValue(emissionType, out var subscriptionSet))
            {
                subscriptionSet.Lock.EnterReadLock();
                try
                {
                    foreach (var subscription in subscriptionSet.Subscriptions)
                    {
                        subscription.Subscriber.HandleMessage(message);
                    }
                }
                finally
                {
                    subscriptionSet.Lock.ExitReadLock();
                }
            }
        }

        return this;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe<TMessage>(IMessageSubscriber<TMessage> subscriber)
        where TMessage : IMessage
    {
        return Subscribe(typeof(TMessage), subscriber);
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(Type messageType, IMessageSubscriber subscriber)
    {
        var subscriptionSet = _subscriptions.GetOrAdd(messageType, t => new(t));
        return subscriptionSet.Add(subscriber);
    }

    private IEnumerable<Type> GetEmissionTypes(Type messageType)
    {
        // always emit the actual type
        yield return messageType;

        // walk class base types first
        if (messageType.IsClass)
        {
            Type? baseType = messageType;
            while ((baseType = baseType?.BaseType) is not null)
            {
                yield return baseType;
                messageType = baseType;
            }
        }

        // then all interfaces
        foreach (var interfaceType in messageType.GetInterfaces())
        {
            yield return interfaceType;
        }
    }

    /// <inheritdoc/>
    public void HandleMessage(IMessage message)
    {
        Emit(message);
    }

    private record SubscriptionSet(Type MessageType)
    {
        public SortedSet<Subscription> Subscriptions { get; } = new(SubscriptionComparer.Instance);
        public ReaderWriterLockSlim Lock { get; } = new();

        public IDisposable Add(IMessageSubscriber subscriber)
        {
            Lock.EnterWriteLock();
            try
            {
                Subscriptions.Add(new(subscriber));
            }
            finally
            {
                Lock.ExitWriteLock();
            }

            return new DisposalAction(() =>
            {
                Lock.EnterWriteLock();
                try
                {
                    Subscriptions.RemoveWhere(s => s.Subscriber == subscriber);
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            });
        }
    }

    private class Subscription(IMessageSubscriber subscriber)
    {
        public IMessageSubscriber Subscriber { get; } = subscriber;
        public sbyte Priority { get; } = subscriber.Priority;
    }

    private class SubscriptionComparer : IComparer<Subscription>
    {
        public static readonly SubscriptionComparer Instance = new();

        public int Compare(Subscription? x, Subscription? y)
        {
            if (x is null || y is null)
            {
                throw new ArgumentNullException(x is null ? nameof(x) : nameof(y));
            }

            return y.Priority.CompareTo(x.Priority);
        }
    }
}
