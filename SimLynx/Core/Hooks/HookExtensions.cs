using System;
using Autofac;
using Autofac.Builder;
using Autofac.Core;

namespace SimLynx.Core.Hooks;

/// <summary>
/// Extension methods related to hooks.
/// </summary>
public static class HookExtensions
{
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
            where THook : Hook<TPayload>
        {
            return @this.RegisterType<THook>().AsSelf().As<Hook<TPayload>>().SingleInstance();
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
            return @this.RegisterHook(payloadType, typeof(Hook<>));
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
            var baseHookType = typeof(Hook<>).MakeGenericType(payloadType);
            if (!hookType.IsAssignableTo(baseHookType))
            {
                throw new ArgumentException(
                    $"The provided hook type '{hookType}' must be assignable to '{baseHookType}'.",
                    nameof(hookType)
                );
            }

            return @this.RegisterType(hookType).AsSelf().As(baseHookType).SingleInstance();
        }
    }

    extension<TPayload, TActivatorData, TStyle>(IRegistrationBuilder<Hook<TPayload>, TActivatorData, TStyle> @this)
    {
        /// <summary>
        /// Pipes a <typeparamref name="TPayload"/> hook to a <typeparamref name="TTarget"/> hook by subscribing to the
        /// former and piping it to the latter, using the provided <paramref name="mapFunc"/> to convert the payload.
        /// </summary>
        /// <typeparam name="TTarget">The type of the payload for the target hook.</typeparam>
        /// <param name="mapFunc">The function to map the payload from the source hook to the target hook.</param>
        /// <returns>The registration builder for the hook.</returns>
        public IRegistrationBuilder<Hook<TPayload>, TActivatorData, TStyle> WithPipe<TTarget>(
            Func<TPayload, TTarget> mapFunc
        )
        {
            IDisposable? sub = null;
            return @this
                .OnActivated(
                    (e) =>
                    {
                        var targetHook = e.Context.Resolve<Hook<TTarget>>();
                        sub = e.Instance.Subscribe((payload, context) => targetHook.Invoke(mapFunc(payload), context));
                    }
                )
                .OnRelease((e) => sub?.Dispose());
        }
    }

    extension<TPayload, TActivatorData, TStyle>(IRegistrationBuilder<Hook<TPayload>, TActivatorData, TStyle> @this)
        where TActivatorData : ReflectionActivatorData
    {
        /// <summary>
        /// Applies a particular <paramref name="deliveryStrategy"/> to the hook being registered.
        /// </summary>
        /// <param name="deliveryStrategy">The delivery strategy to apply.</param>
        /// <returns>The registration builder for the hook.</returns>
        public IRegistrationBuilder<Hook<TPayload>, TActivatorData, TStyle> WithDeliveryStrategy(
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
        public IRegistrationBuilder<Hook<TPayload>, TActivatorData, TStyle> WithDeliveryStrategy(
            Type deliveryStrategyType
        )
        {
            return @this.WithParameter(
                new ResolvedParameter(
                    (pi, ctx) => pi.ParameterType == typeof(IHookDeliveryStrategy),
                    (pi, ctx) => ctx.Resolve(deliveryStrategyType)
                )
            );
        }
    }
}
