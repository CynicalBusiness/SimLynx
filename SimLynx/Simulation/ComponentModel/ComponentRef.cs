using System;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Reference to a <typeparamref name="T"/> component on the same entity.
/// </summary>
/// <typeparam name="T">The type of the component, or base/interface it implements.</typeparam>
public readonly struct ComponentRef<T>
{
    /// <summary>
    /// The component referenced by this <see cref="ComponentRef{T}"/>.
    /// </summary>
    public T Component => throw new NotImplementedException();
}
