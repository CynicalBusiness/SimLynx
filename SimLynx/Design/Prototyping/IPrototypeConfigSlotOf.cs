using System.Collections.Generic;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// A slot for <typeparamref name="TConfig"/> prototype configurations.
/// </summary>
/// <typeparam name="TConfig">The type of configuration this slot accepts.</typeparam>
public interface IPrototypeConfigSlotOf<out TConfig> : IPrototypeConfigSlot
    where TConfig : IPrototypeConfig
{
    /// <inheritdoc cref="IPrototypeConfigSlot.this[string]"/>
    public new TConfig? this[string name] { get; }

    /// <inheritdoc cref="IPrototypeConfigSlot.GetOwn"/>
    public new IEnumerable<TConfig> GetOwn();

    IPrototypeConfig? IPrototypeConfigSlot.this[string name] => this[name];
    IEnumerable<IPrototypeConfig> IPrototypeConfigSlot.GetOwn() => (IEnumerable<IPrototypeConfig>)GetOwn();
}
