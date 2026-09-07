using System.Collections.Generic;

namespace SimLynx.Design.Prototyping.Parameters;

/// <summary>
/// Helpers for working with parameter config slots.
/// </summary>
public static class ParameterConfigSlot
{
    /// <summary>
    /// Standard slot ID for parameter configs.
    /// </summary>
    public static Identifier SlotId { get; } = new("parameters");
}

/// <summary>
/// A slot for configuring dependency injection parameters for subject instances.
/// </summary>
/// <remarks>
/// This slot's identifiers do <em>not</em> have any bearing on the parameters themselves, but are only used to identify
/// a particular parameter's config within the slot.
/// </remarks>
/// <typeparam name="TSubject">The prototype subject type.</typeparam>
/// <param name="prototype">The prototype instance this slot belongs to.</param>
public class ParameterConfigSlot<TSubject>(IPrototype<TSubject> prototype)
    : PrototypeConfigSlot<TSubject, IParameterConfig<TSubject>>(prototype)
    where TSubject : class, IPrototypeSubject
{
    private readonly Dictionary<Identifier, IParameterConfig<TSubject>> _configs = [];

    /// <inheritdoc/>
    protected override IParameterConfig<TSubject>? Create(Identifier id)
    {
        if (!_configs.TryGetValue(id, out var config))
        {
            config = new ParameterConfig<TSubject>(id);
            _configs[id] = config;
        }
        return config;
    }
}
