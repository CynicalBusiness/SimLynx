using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;

namespace SimLynx.Core.Hooks;

internal class HooksModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<SerialHookDeliveryStrategy>().As<IHookDeliveryStrategy>().SingleInstance();

        // scan for hook handlers and register them to the relevant hooks
        builder.RegisterBuildCallback(
            (context) =>
            {
                var subscriptions = new List<IDisposable>();

                foreach (var reg in context.ComponentRegistry.Registrations)
                {
                    if (!reg.Activator.LimitType.IsAssignableTo(typeof(IHookHandler)))
                    {
                        continue;
                    }

                    foreach (var service in reg.Services.OfType<TypedService>())
                    {
                        if (
                            !service.ServiceType.IsGenericType
                            || service.ServiceType.GetGenericTypeDefinition() != typeof(IHookHandler<>)
                        )
                        {
                            continue;
                        }

                        var handlerType = service.ServiceType.GetGenericArguments()[0];
                        var handler = (IHookHandler)
                            context.ResolveComponent(new ResolveRequest(service, new(reg.ResolvePipeline, reg), []));

                        var hookType = typeof(Hook<>).MakeGenericType(handlerType);
                        var hook = context.Resolve(hookType);

                        var sub =
                            hook.GetType().GetMethod(nameof(Hook<>.Subscribe))?.Invoke(hook, [handler]) as IDisposable;
                        if (sub is not null)
                        {
                            subscriptions.Add(sub);
                        }
                    }
                }

                context.CurrentScopeEnding += (_, _) =>
                {
                    // clean up subscriptions when the scope ends
                    foreach (var sub in subscriptions)
                    {
                        sub.Dispose();
                    }
                };
            }
        );
    }
}
