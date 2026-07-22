using System.Collections.Generic;
using SimLynx.Design.Prototyping.Blueprints;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Base implementation of a prototype config slot.
/// </summary>
/// <typeparam name="TSubject">The concrete subject type being configured.</typeparam>
/// <typeparam name="TConfig">The type of configuration this slot accepts.</typeparam>
/// <param name="prototype">The prototype that owns this config slot.</param>
/// <param name="slotId">The ID of the config slot.</param>
public abstract class PrototypeConfigSlot<TSubject, TConfig>(IPrototype<TSubject> prototype, Symbol slotId)
    : IPrototypeConfigSlot<TSubject, TConfig>
    where TSubject : class, IPrototypeSubject
    where TConfig : class, IPrototypeConfig<TSubject>
{
    /// <summary>
    /// The prototype that owns this config slot.
    /// </summary>
    protected IPrototype<TSubject> Prototype { get; } = prototype;

    /// <summary>
    /// Dictionary of configurations in this slot, keyed by their name.
    /// </summary>
    protected Dictionary<string, TConfig> Configs { get; } = [];

    /// <inheritdoc/>
    public Symbol Id { get; } = slotId;

    /// <inheritdoc/>
    public virtual TConfig? this[string name]
    {
        get
        {
            if (Configs.TryGetValue(name, out var config))
            {
                return config;
            }

            if ((config = Create(name)) is not null)
            {
                Configs[name] = config;
                return config;
            }

            return null;
        }
    }

    /// <inheritdoc/>
    public virtual bool IsSupported => true;

    /// <inheritdoc/>
    public virtual IEnumerable<TConfig> GetOwn()
    {
        return Configs.Values;
    }

    /// <summary>
    /// Configures this slot for the given <paramref name="builder"/>. This is called during prototype compilation to allow
    /// the slot to configure the blueprint with any necessary information.
    /// </summary>
    /// <param name="builder">The blueprint builder to configure.</param>
    public abstract void Configure(IBlueprintBuilder<TSubject> builder);

    /// <summary>
    /// Attempts to create a configuration for the given <paramref name="name"/> in this slot. If it cannot be created,
    /// may return <c>null</c> to indicate configuration is not supported, but is not an error.
    /// </summary>
    /// <param name="name">The name of the configuration to create.</param>
    /// <returns>The created configuration, or <c>null</c> if it cannot be created.</returns>
    protected abstract TConfig? Create(string name);
}
