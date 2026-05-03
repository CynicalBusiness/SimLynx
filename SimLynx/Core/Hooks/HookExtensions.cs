using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Microsoft.Extensions.Logging;

namespace SimLynx.Core.Hooks;

/// <summary>
/// Extension methods related to hooks.
/// </summary>
public static class HookExtensions
{
    extension<TPayload>(IHookable<TPayload> @this)
    {
        /// <inheritdoc cref="IHookable{TPayload}.Subscribe(IHookHandler{TPayload})"/>
        /// <param name="handler">The handler to subscribe.</param>
        /// <param name="priority">Overrides the default priority of the handler.</param>
        public IDisposable Subscribe(IHookHandler<TPayload> handler, sbyte priority = Priorities.Default)
        {
            return @this.Subscribe(new HookHandler<TPayload>(handler.HandleHook, handler.Source, priority));
        }

        /// <summary>
        /// Helper for subscribing to a hook with a simple delegate, using the provided priority or the default if not
        /// specified.
        /// </summary>
        /// <param name="handler">The handler delegate to subscribe.</param>
        /// <param name="priority">The priority of the handler.</param>
        /// <param name="source">The source object for the handler.</param>
        /// <returns>A disposable to unsubscribe the handler.</returns>
        public IDisposable Subscribe(
            HookHandlerFunc<TPayload> handler,
            object? source = null,
            sbyte priority = Priorities.Default
        )
        {
            return @this.Subscribe(new HookHandler<TPayload>(handler, source, priority));
        }

        /// <summary>
        /// Pipes the result of this hook to the given <paramref name="targetHook"/>, invoking the target hook with
        /// each invocation of this hook and returning the invocation task from the target after mapping the payload
        /// with the provided <paramref name="mapFunc"/>.
        /// </summary>
        /// <typeparam name="TTarget">The target hook type</typeparam>
        /// <param name="targetHook">The target hook to pipe to.</param>
        /// <param name="mapFunc">A function to map the payload from this hook to the target hook.</param>
        /// <param name="priority">The priority of the subscription.</param>
        /// <returns>A disposable to unsubscribe the pipe.</returns>
        public IDisposable PipeTo<TTarget>(
            IHookInvocable<TTarget> targetHook,
            Func<TPayload, TTarget> mapFunc,
            sbyte priority = Priorities.Default
        )
        {
            return @this.Subscribe((payload, context) => targetHook.Invoke(mapFunc(payload), context), priority);
        }
    }

    /// <summary>
    /// Pipes the result of this hook to the given <paramref name="targetHook"/>, invoking the target hook with
    /// each invocation of this hook and returning the invocation task from the target.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload for this hook.</typeparam>
    /// <typeparam name="TTarget">The type of the payload for the target hook.</typeparam>
    /// <param name="this">The hook to pipe from.</param>
    /// <param name="targetHook">The target hook to pipe to.</param>
    /// <param name="priority">The priority of the subscription.</param>
    /// <returns>A disposable to unsubscribe the pipe.</returns>
    public static IDisposable PipeTo<TPayload, TTarget>(
        this IHookable<TPayload> @this,
        IHookInvocable<TTarget> targetHook,
        sbyte priority = Priorities.Default
    )
        where TPayload : TTarget
    {
        return @this.PipeTo(targetHook, payload => payload, priority);
    }

    extension(ContainerBuilder @this)
    {
        /// <summary>
        /// Registers a hook of type <typeparamref name="TPayload"/>.
        /// </summary>
        /// <remarks>
        /// By default, this registers the hook as a singleton, but this can be overridden by using the returned
        /// builder.
        /// </remarks>
        /// <typeparam name="TPayload">The type of the payload for the hook.</typeparam>
        /// <returns>The registration builder for the hook.</returns>
        public IRegistrationBuilder<
            Hook<TPayload>,
            ConcreteReflectionActivatorData,
            SingleRegistrationStyle
        > RegisterHook<TPayload>()
        {
            return @this.RegisterHook<TPayload, Hook<TPayload>>();
        }

        /// <summary>
        /// Registers a <typeparamref name="THook"/> for the specified <typeparamref name="TPayload"/>.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload for the hook.</typeparam>
        /// <typeparam name="THook">The type of the hook to register.</typeparam>
        /// <returns>The registration builder for the hook.</returns>
        public IRegistrationBuilder<THook, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterHook<
            TPayload,
            THook
        >()
            where THook : IHook<TPayload>
        {
            return @this.RegisterType<THook>().ApplyHookDefaults(typeof(TPayload));
        }

        /// <summary>
        /// Registers a hook for the specified <paramref name="payloadType"/>.
        /// </summary>
        /// <remarks>
        /// By default, this registers the hook as a singleton, but this can be overridden by using the returned
        /// builder.
        /// </remarks>
        /// <param name="payloadType">The type of the payload for the hook.</param>
        /// <returns>The registration builder for the hook.</returns>
        public IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterHook(
            Type payloadType
        )
        {
            return @this.RegisterHook(payloadType, typeof(IHook<>));
        }

        /// <summary>
        /// Registers a hook of the specified <paramref name="hookType"/> for the specified
        /// <paramref name="payloadType"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="hookType"/> may be a generic type definition (i.e. open generic); if so, it will be
        /// closed with the payload type.
        /// </remarks>
        /// <param name="payloadType">The type of the payload for the hook.</param>
        /// <param name="hookType">The type of the hook to register.</param>
        /// <returns>The registration builder for the hook.</returns>
        /// <exception cref="ArgumentException">If the hook/payload type is invalid</exception>
        public IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterHook(
            Type payloadType,
            Type hookType
        )
        {
            if (hookType.IsGenericTypeDefinition)
            {
                hookType = hookType.MakeGenericType(payloadType);
            }
            var baseHookType = typeof(IHook<>).MakeGenericType(payloadType);
            if (!hookType.IsAssignableTo(baseHookType))
            {
                throw new ArgumentException(
                    $"The provided hook type '{hookType}' must be assignable to '{baseHookType}'.",
                    nameof(hookType)
                );
            }

            return @this.RegisterType(hookType).ApplyHookDefaults(payloadType);
        }
    }

    extension<TLimit>(IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, SingleRegistrationStyle> @this)
    {
        private IRegistrationBuilder<
            TLimit,
            ConcreteReflectionActivatorData,
            SingleRegistrationStyle
        > ApplyHookDefaults(Type payloadType)
        {
            return @this
                .AsSelf()
                .As<IHook>()
                .As(typeof(IHook<>).MakeGenericType(payloadType))
                .As(typeof(IHookable<>).MakeGenericType(payloadType))
                .As(typeof(IHookInvocable<>).MakeGenericType(payloadType))
                .SingleInstance();
        }
    }

    extension<TPayload, TActivatorData, TStyle>(IRegistrationBuilder<IHookable<TPayload>, TActivatorData, TStyle> @this)
    {
        /// <summary>
        /// Pipes a <typeparamref name="TPayload"/> hook to a <typeparamref name="TTarget"/> hook by subscribing to the
        /// former and piping it to the latter, using the provided <paramref name="mapFunc"/> to convert the payload.
        /// </summary>
        /// <typeparam name="TTarget">The type of the payload for the target hook.</typeparam>
        /// <param name="mapFunc">The function to map the payload from the source hook to the target hook.</param>
        /// <returns>The registration builder for the hook.</returns>
        public IRegistrationBuilder<IHookable<TPayload>, TActivatorData, TStyle> WithPipe<TTarget>(
            Func<TPayload, TTarget> mapFunc
        )
        {
            IDisposable? sub = null;
            return @this
                .OnActivated(
                    (e) =>
                    {
                        var targetHook = e.Context.Resolve<IHookInvocable<TTarget>>();
                        sub = e.Instance.PipeTo(targetHook, mapFunc);
                    }
                )
                .OnRelease((e) => sub?.Dispose());
        }
    }

    extension<TPayload, TActivatorData, TStyle>(IRegistrationBuilder<IHook<TPayload>, TActivatorData, TStyle> @this)
        where TActivatorData : ReflectionActivatorData
    {
        /// <summary>
        /// Applies a particular <paramref name="deliveryStrategy"/> to the hook being registered.
        /// </summary>
        /// <param name="deliveryStrategy">The delivery strategy to apply.</param>
        /// <returns>The registration builder for the hook.</returns>
        public IRegistrationBuilder<IHook<TPayload>, TActivatorData, TStyle> WithDeliveryStrategy(
            IHookDeliveryStrategy deliveryStrategy
        )
        {
            return @this.WithParameter(new TypedParameter(typeof(IHookDeliveryStrategy), deliveryStrategy));
        }

        /// <summary>
        /// Applies a particular <paramref name="deliveryStrategyType"/> to the hook being registered, resolved from
        /// the hook's scope.
        /// </summary>
        /// <param name="deliveryStrategyType">The delivery strategy to apply.</param>
        /// <returns>The registration builder for the hook.</returns>
        public IRegistrationBuilder<IHook<TPayload>, TActivatorData, TStyle> WithDeliveryStrategy(
            Type deliveryStrategyType
        )
        {
            return @this.WithParameter(
                new ResolvedParameter(
                    (pi, ctx) => deliveryStrategyType.IsAssignableTo(pi.ParameterType),
                    (pi, ctx) => ctx.Resolve(deliveryStrategyType)
                )
            );
        }
    }

    extension<TLimit, TActivatorData, TStyle>(IRegistrationBuilder<TLimit, TActivatorData, TStyle> @this)
        where TLimit : notnull
    {
        /// <summary>
        /// Helper for subscribing to a hook when the registered type is activated, then disposing automatically when
        /// the released.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload for the hook.</typeparam>
        /// <param name="getHandler">A function to get the handler to subscribe from the activation context.</param>
        /// <returns>The registration builder for the hook.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TStyle> OnHook<TPayload>(
            Func<IActivatedEventArgs<TLimit>, IHookHandler<TPayload>> getHandler
        )
        {
            var dict = new ConcurrentDictionary<TLimit, IDisposable>();
            @this
                .OnActivated(
                    (@event) =>
                    {
                        var handler = getHandler(@event);
                        var hook = @event.Context.Resolve<IHookable<TPayload>>();
                        dict[@event.Instance] = hook.Subscribe(handler);
                    }
                )
                .OnRelease(
                    (limit) =>
                    {
                        if (dict.TryRemove(limit, out var sub))
                        {
                            sub.Dispose();
                        }
                    }
                );
            return @this;
        }
    }
}
