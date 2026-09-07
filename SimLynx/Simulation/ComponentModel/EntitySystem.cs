using System.Collections.Generic;
using System.Collections.Immutable;
using SimLynx.Simulation.ComponentModel.Prototyping;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// A system that contains and manages a single entity tree.
/// </summary>
public class EntitySystem(IComponentPrototype<Entity> prototype) : ISystem
{
    private readonly ImmutableDictionary<ComponentAddress, ComponentNode> _components;

    internal readonly struct ComponentNode(ComponentInfo info, Component instance)
    {
        public ComponentInfo Info { get; } = info;
        public Component Instance { get; } = instance;

        public ComponentAddress Address => Info.Address;
    }

    internal sealed class ComponentNodeComparer : IEqualityComparer<ComponentNode>
    {
        public static readonly ComponentNodeComparer Instance = new();

        private ComponentNodeComparer() { }

        public bool Equals(ComponentNode x, ComponentNode y) => x.Address.Equals(y.Address);

        public int GetHashCode(ComponentNode obj) => obj.Address.GetHashCode();
    }
}
