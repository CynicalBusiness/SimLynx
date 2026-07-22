using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using SimLynx.Design.Prototyping;
using SimLynx.Design.Prototyping.Blueprints;

namespace SimLynx.Simulation.ComponentModel.Prototyping;

/// <summary>
/// Static helpers for component config slots.
/// </summary>
public static class ComponentConfigSlot
{
    /// <summary>
    /// Separator character used to separate the component type name from its given name.
    /// </summary>
    public const char CONFIG_NAME_SEPARATOR = ':';

    /// <summary>
    /// The slot ID for the component config slot.
    /// </summary>
    public static readonly Symbol SLOT_ID = "Components";

    /// <summary>
    /// Attempts to parse a component config name into its component type and, if present, given name.
    /// </summary>
    /// <param name="inputName">The config name to parse.</param>
    /// <param name="componentType">The parsed component type, if successful.</param>
    /// <param name="givenName">The parsed given name, if present.</param>
    /// <returns><c>true</c> if the config name was successfully parsed; otherwise, <c>false</c>.</returns>
    public static bool TryParseConfigName(
        string inputName,
        [MaybeNullWhen(false)] out IComponentType componentType,
        out string? givenName
    )
    {
        var sepIdx = inputName.IndexOf(CONFIG_NAME_SEPARATOR);

        string typeName;
        if (sepIdx < 0)
        {
            givenName = null;
            typeName = inputName;
        }
        else
        {
            typeName = inputName[..sepIdx];
            givenName = inputName[(sepIdx + 1)..];
        }

        componentType = ComponentTypes.Find(typeName);
        return componentType is not null;
    }

    /// <summary>
    /// Gets the config name for a component config, given its <paramref name="componentType"/> and optional
    /// <paramref name="givenName"/>.
    /// </summary>
    /// <param name="componentType">The component type.</param>
    /// <param name="givenName">The given name of the component, if any.</param>
    /// <returns>The config name for the component config.</returns>
    public static string GetConfigName(IComponentType componentType, string? givenName = null)
    {
        var typeName = componentType.Type.FullName ?? componentType.Type.Name;

        return givenName is null ? typeName : $"{typeName}{CONFIG_NAME_SEPARATOR}{givenName}";
    }
}

/// <summary>
/// Prototype config slot for configuring components on a <typeparamref name="TSubject"/> component.
/// </summary>
/// <typeparam name="TSubject">The type of the component being configured.</typeparam>
/// <param name="slotId">The ID of the config slot.</param>
/// <param name="prototype">The prototype being configured.</param>
/// <param name="prototypeResolver">The factory for creating component prototype registries.</param>
public class ComponentConfigSlot<TSubject>(
    Symbol slotId,
    IComponentPrototype<TSubject> prototype,
    PrototypeResolver<Component> prototypeResolver
) : PrototypeConfigSlot<TSubject, IComponentConfig<TSubject>>(prototype, slotId)
    where TSubject : Component
{
    private readonly MethodInfo _genericCreateMethod = typeof(ComponentConfigSlot<TSubject>).GetMethod(
        nameof(CreateComponentConfig),
        BindingFlags.NonPublic | BindingFlags.Instance,
        null,
        [],
        null
    )!;

    /// <inheritdoc/>
    public override void Configure(IBlueprintBuilder<TSubject> builder)
    {
        // TODO
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override IComponentConfig<TSubject>? Create(string name)
    {
        if (!ComponentConfigSlot.TryParseConfigName(name, out var componentType, out var givenName))
        {
            return null;
        }

        return _genericCreateMethod.MakeGenericMethod(componentType.Type).Invoke(this, [givenName])
            as IComponentConfig<TSubject>;
    }

    private ComponentConfig<TSubject, TComponent> CreateComponentConfig<TComponent>(string? givenName)
        where TComponent : Component
    {
        return new ComponentConfig<TSubject, TComponent>(prototypeResolver, givenName);
    }
}
