using System.Collections.Generic;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// Equality comparer for <see cref="IPrototypeConfigSlot"/> instances based on their <see cref="IPrototypeConfigSlot.Id"/>.
/// </summary>
public class PrototypeConfigSlotIdComparer<TSlot> : IEqualityComparer<TSlot>
    where TSlot : IPrototypeConfigSlot
{
    /// <summary>
    /// The default instance of <see cref="PrototypeConfigSlotIdComparer{TSlot}"/> for use in equality comparisons.
    /// </summary>
    public static PrototypeConfigSlotIdComparer<TSlot> Default { get; } = new();

    /// <inheritdoc/>
    public bool Equals(TSlot? x, TSlot? y)
    {
        if (x is null && y is null)
        {
            return true;
        }
        if (x is null || y is null)
        {
            return false;
        }
        return x.Id == y.Id;
    }

    /// <inheritdoc/>
    public int GetHashCode(TSlot obj)
    {
        return obj.Id.GetHashCode();
    }
}

/// <inheritdoc cref="PrototypeConfigSlotIdComparer{TSlot}"/>
public class PrototypeConfigSlotIdComparer : PrototypeConfigSlotIdComparer<IPrototypeConfigSlot> { }
