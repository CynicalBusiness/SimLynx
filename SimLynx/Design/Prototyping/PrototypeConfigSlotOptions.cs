using System;
using System.Linq;
using SimLynx.Core;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Definition of a prototype configuration slot in the catalog.
/// </summary>
public class PrototypeConfigSlotDef
{
    /// <summary>
    /// Creates a new catalog entry for a prototype configuration slot.
    /// </summary>
    /// <param name="slotId">ID of the slot.</param>
    /// <param name="typeDef">Open-generic type definition of the slot.</param>
    /// <exception cref="ArgumentException">If the slot type or symbol is invalid</exception>
    public PrototypeConfigSlotDef(Identifier slotId, Type typeDef)
    {
        if (!slotId.IsSpecified)
        {
            throw new ArgumentException("Slot ID cannot be empty or unspecified.", nameof(slotId));
        }

        if (!typeDef.IsGenericTypeDefinition || !typeDef.IsGenericTypeOf(typeof(IPrototypeConfigSlotFor<>)))
        {
            throw new ArgumentException(
                $"The provided slot type {typeDef} must be an open generic that implements IPrototypeConfigSlotFor<>.",
                nameof(typeDef)
            );
        }

        Id = slotId;
        TypeDef = typeDef;
    }

    /// <summary>
    /// ID of the configured slot.
    /// </summary>
    public Identifier Id { get; }

    /// <summary>
    /// Open-generic type definition of the slot, to be closed with a specific subject type.
    /// </summary>
    public Type TypeDef { get; }

    /// <summary>
    /// Indicates this slot should be eagerly initialized when the prototype is created, rather than lazily when
    /// a config is first requested from it.
    /// </summary>
    public bool Eager { get; init; } = false;

    /// <summary>
    /// Additional types that can be used to resolve the config slot configured by this entry.
    /// </summary>
    /// <remarks>
    /// Config-type aliases are not required to be unique across slot definitions. Resolution tries exact
    /// closed-generic aliases before aliases for an open-generic type definition. Within either group, later
    /// registrations are tried first and slot priority is ignored; unsupported candidates fall back to earlier
    /// registrations.
    /// </remarks>
    public Type[] ConfigTypes
    {
        get;
        init
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            var supportedConfigTypes = TypeDef
                .GetGenericTypesOf(typeof(IPrototypeConfigSlotOf<>))
                .Where(t => !t.IsGenericTypeDefinition)
                .Select(t => t.GetGenericArguments()[0])
                .ToArray();

            foreach (var type in value)
            {
                if (type is null)
                {
                    throw new ArgumentException("Config types cannot contain null values.", nameof(value));
                }

                if (
                    !supportedConfigTypes.Any(t =>
                        type.IsGenericTypeDefinition ? t.IsGenericTypeOf(type) : t.IsAssignableTo(type)
                    )
                )
                {
                    throw new ArgumentException(
                        $"Config type {type} is not supported by slot type {TypeDef}. Supported config types are: {string.Join<Type>(", ", supportedConfigTypes)}",
                        nameof(value)
                    );
                }
            }

            field = value;
        }
    } = [];

    /// <summary>
    /// Priority of the slot. Higher-priority slots will be configured first. Equal-priority slots retain registration
    /// order. Priority does not affect lookup through <see cref="ConfigTypes"/>.
    /// </summary>
    public sbyte Priority { get; init; } = Priorities.Default;
}
