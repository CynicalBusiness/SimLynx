using System.Collections.Generic;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// A slot for prototype configurations.
/// </summary>
public interface IPrototypeConfigSlot
{
    /// <summary>
    /// Gets the name of this config slot.
    /// </summary>
    public Symbol Id { get; }

    /// <summary>
    /// Gets whether this config slot is supported in its current context.
    /// </summary>
    public bool IsSupported { get; }

    /// <summary>
    /// Gets the configuration for the given <paramref name="name"/> in this slot. If it does not exist, this will
    /// attempt to create it, returning <c>null</c> if it cannot be created (such as if it is not valid/supported).
    /// </summary>
    /// <param name="name">The name of the configuration.</param>
    /// <returns>The configuration for the given name.</returns>
    public IPrototypeConfig? this[string name] { get; }

    /// <summary>
    /// Gets all configurations owned by this slot.
    /// </summary>
    /// <returns>All configurations in this slot.</returns>
    public IEnumerable<IPrototypeConfig> GetOwn();
}

/// <summary>
/// A slot for <typeparamref name="TConfig"/> prototype configurations on a <typeparamref name="TSubject"/> prototype.
/// </summary>
/// <typeparam name="TSubject">The type of the prototype subject.</typeparam>
/// <typeparam name="TConfig">The type of configuration this slot accepts.</typeparam>
public interface IPrototypeConfigSlot<in TSubject, out TConfig>
    : IPrototypeConfigSlotFor<TSubject>,
        IPrototypeConfigSlotOf<TConfig>
    where TConfig : IPrototypeConfig
    where TSubject : class, IPrototypeSubject { }
