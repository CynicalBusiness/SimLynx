namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Meta-type for a component.
/// </summary>
public interface IComponentType : IMetaType { }

/// <summary>
/// Meta-type for a <typeparamref name="TComponent"/> component.
/// </summary>
/// <typeparam name="TComponent">The type of component represented by this meta-type.</typeparam>
public interface IComponentType<out TComponent> : IComponentType, IMetaType<TComponent>
    where TComponent : Component { }
