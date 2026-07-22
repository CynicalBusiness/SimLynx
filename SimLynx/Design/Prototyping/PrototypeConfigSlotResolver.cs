using System;
using System.Diagnostics.CodeAnalysis;
using Autofac;
using Autofac.Core;
using SimLynx.Core;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Resolves config slots for a prototype using an Autofac container.
/// </summary>
/// <typeparam name="TSubject">The type of the prototype subject.</typeparam>
/// <param name="prototype">The prototype instance.</param>
/// <param name="container">The Autofac container.</param>
public class PrototypeConfigSlotResolver<TSubject>(IPrototype<TSubject> prototype, IComponentContext container)
    where TSubject : class, IPrototypeSubject
{
    /// <summary>
    /// Factory delegate for creating a <see cref="PrototypeConfigSlotResolver{TSubject}"/> instance.
    /// </summary>
    /// <param name="prototype">The prototype instance.</param>
    /// <returns>A new <see cref="PrototypeConfigSlotResolver{TSubject}"/> instance.</returns>
    public delegate PrototypeConfigSlotResolver<TSubject> Factory(IPrototype<TSubject> prototype);

    /// <summary>
    /// Attempts to resolve a config slot for the given <paramref name="slotId"/>. If the slot is not found, returns false.
    /// </summary>
    /// <param name="slotId">The ID of the config slot to resolve.</param>
    /// <param name="slot">The resolved config slot, if found; otherwise, null.</param>
    /// <returns>True if the config slot was resolved; otherwise, false.</returns>
    public bool TryResolve(Symbol slotId, [MaybeNullWhen(false)] out IPrototypeConfigSlotFor<TSubject> slot)
    {
        var service = new KeyedService(slotId, typeof(IPrototypeConfigSlotFor<TSubject>));

        if (
            container.TryResolveService(
                service,
                [new LooseTypedParameter(prototype, typeof(IPrototype))],
                out var rawSlot
            )
        )
        {
            slot = (IPrototypeConfigSlotFor<TSubject>)rawSlot;
            return slot.IsSupported;
        }

        slot = null;
        return false;
    }

    /// <summary>
    /// Attempts to resolve a config slot for the given <paramref name="configType"/>. If the slot is not found,
    /// returns false.
    /// </summary>
    /// <param name="configType">The type of the config slot to resolve.</param>
    /// <param name="slot">The resolved config slot, if found; otherwise, null.</param>
    /// <returns>True if the config slot was resolved; otherwise, false.</returns>
    public bool TryResolve(Type configType, [MaybeNullWhen(false)] out IPrototypeConfigSlotFor<TSubject> slot)
    {
        var service = new KeyedService(configType, typeof(IPrototypeConfigSlotFor<TSubject>));

        if (
            container.TryResolveService(
                service,
                [new LooseTypedParameter(prototype, typeof(IPrototype))],
                out var rawSlot
            )
        )
        {
            slot = (IPrototypeConfigSlotFor<TSubject>)rawSlot;
            return slot.IsSupported;
        }

        if (configType.IsGenericType && !configType.IsGenericTypeDefinition)
        {
            return TryResolve(configType.GetGenericTypeDefinition(), out slot);
        }

        slot = null;
        return false;
    }

    /// <summary>
    /// Attempts to resolve a config slot for the given <typeparamref name="TConfig"/> type. If the slot is not found,
    /// returns false.
    /// </summary>
    /// <typeparam name="TConfig">The type of the config slot to resolve.</typeparam>
    /// <param name="slot">The resolved config slot, if found; otherwise, null.</param>
    /// <returns>True if the config slot was resolved; otherwise, false.</returns>
    public bool TryResolve<TConfig>([MaybeNullWhen(false)] out IPrototypeConfigSlot<TSubject, TConfig> slot)
        where TConfig : class, IPrototypeConfig
    {
        if (
            TryResolve(typeof(TConfig), out var rawSlot) && rawSlot is IPrototypeConfigSlot<TSubject, TConfig> typedSlot
        )
        {
            slot = typedSlot;
            return true;
        }

        slot = null;
        return false;
    }
}
