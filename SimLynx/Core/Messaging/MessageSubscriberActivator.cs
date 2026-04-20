using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;

namespace SimLynx.Core.Messaging;

// helper class to discover and subscribe all IMessageSubscriber<> implementations in a lifetime scope
internal class MessageSubscriberActivator(IMessageBus messageBus, IComponentContext componentContext)
    : IStartable,
        IDisposable
{
    public static readonly MethodInfo SubscribeMethod = typeof(MessageSubscriberActivator).GetMethod(
        nameof(Subscribe),
        BindingFlags.NonPublic | BindingFlags.Instance
    )!;

    private readonly List<IDisposable> _subscriptions = [];

    public void Start()
    {
        foreach (var (subscriber, messageType) in GetRegisteredSubscribers())
        {
            var sub = (IDisposable)SubscribeMethod.MakeGenericMethod(messageType).Invoke(this, [subscriber])!;
            _subscriptions.Add(sub);
        }
    }

    private IDisposable Subscribe<TPayload>(IMessageSubscriber<TPayload> subscriber)
    {
        return messageBus.Subscribe(subscriber);
    }

    private IEnumerable<(IMessageSubscriber Subscriber, Type MessageType)> GetRegisteredSubscribers()
    {
        // get all registrations which are *registered* as closed types of IMessageSubscriber<>
        // we don't want to resolve anything that might be assignable but isn't registered as such
        foreach (IComponentRegistration reg in componentContext.ComponentRegistry.Registrations)
        {
            if (
                !reg.Activator.LimitType.IsAssignableTo(typeof(IMessageSubscriber))
                || reg.Activator.LimitType.IsAssignableTo(typeof(IMessageBus)) // ignore message buses (let's not make infinite recursion, yeah?)
            )
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
                    componentContext.ResolveComponent(new ResolveRequest(service, new(reg.ResolvePipeline, reg), []));
                yield return (subscriber, messageType);
            }
        }
    }

    public void Dispose()
    {
        foreach (var sub in _subscriptions)
        {
            sub.Dispose();
        }
        _subscriptions.Clear();
    }
}
