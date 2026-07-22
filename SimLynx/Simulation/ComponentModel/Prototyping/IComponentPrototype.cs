using SimLynx.Design.Prototyping;

namespace SimLynx.Simulation.ComponentModel.Prototyping;

/// <summary>
/// A general prototype that creates <see cref="Component"/> instances.
/// </summary>
public interface IComponentPrototype : IPrototype { }

/// <summary>
/// A prototype <typeparamref name="TComponent"/> component instances.
/// </summary>
/// <typeparam name="TComponent">The type of component</typeparam>
public interface IComponentPrototype<out TComponent> : IComponentPrototype, IPrototype<TComponent>
    where TComponent : Component { }
