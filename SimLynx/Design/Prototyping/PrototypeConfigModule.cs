using System;
using System.Linq;
using Autofac;
using Autofac.Builder;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Module which registers a prototype configuration slot type.
/// </summary>
public class PrototypeConfigModule : Module
{
    private static void RegisterConfigType(
        IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> registration,
        Type configType
    )
    {
        if (!configType.IsSubclassOfGenericDefinition(typeof(IPrototypeConfigSlotFor<>), typeof(IPrototypeConfigSlot)))
            if (configType.IsGenericType && !configType.IsGenericTypeDefinition)
            {
                registration = registration.Keyed(
                    configType.GetGenericTypeDefinition(),
                    typeof(IPrototypeConfigSlotFor<>)
                );
            }

        registration.Keyed(configType, typeof(IPrototypeConfigSlotFor<>));
    }

    /// <summary>
    /// Creates a new module to register a prototype config slot of the given <paramref name="configSlotType"/>
    /// and <paramref name="slotId"/>.
    /// </summary>
    /// <remarks>
    /// The <paramref name="configSlotType"/> is expected to be an open generic type that implements
    /// <see cref="IPrototypeConfigSlotFor{TSubject}"/> and accepts one generic type parameter.
    /// </remarks>
    /// <param name="configSlotType">The type definition of the config slot.</param>
    /// <param name="slotId">The identifier of the config slot.</param>
    public PrototypeConfigModule(Type configSlotType, Symbol slotId)
    {
        ArgumentNullException.ThrowIfNull(configSlotType, nameof(configSlotType));

        if (slotId == Symbol.Empty)
        {
            throw new ArgumentException("Slot ID cannot be empty.", nameof(slotId));
        }

        if (
            !configSlotType.IsGenericTypeDefinition
            || !configSlotType
                .GetInterfaces()
                .Any(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IPrototypeConfigSlotFor<>))
            || configSlotType.GetGenericArguments().Length != 1
        )
        {
            throw new ArgumentException(
                $"The provided slot type {configSlotType} must be an open generic that implements IPrototypeConfigSlotFor<> and accepts one generic type parameter.",
                nameof(configSlotType)
            );
        }

        SlotId = slotId;
        ConfigSlotType = configSlotType;
    }

    /// <summary>
    /// ID of the config slot to register.
    /// </summary>
    public Symbol SlotId { get; }

    /// <summary>
    /// The type or generic type definition of the config slot to register.
    /// </summary>
    public Type ConfigSlotType { get; }

    /// <summary>
    /// Additional types that can be used to resolve the config slot configured by this module.
    /// </summary>
    /// <remarks>
    /// These types may be interfaces, classes, or generic type definitions of such. Closed generic types will also
    /// be registered as their generic type definitions as well.
    /// </remarks>
    public Type[] ConfigTypes { get; init; } = [];

    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        var registration = builder
            .RegisterGeneric(ConfigSlotType)
            .Keyed(SlotId, typeof(IPrototypeConfigSlotFor<>))
            .WithParameter(new TypedParameter(typeof(Symbol), SlotId))
            .InstancePerDependency();

        foreach (var configType in ConfigTypes)
        {
            RegisterConfigType(registration, configType);
        }
    }
}
