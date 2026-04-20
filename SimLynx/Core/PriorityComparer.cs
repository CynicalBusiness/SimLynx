using System.Collections.Generic;

namespace SimLynx.Core;

/// <summary>
/// Comparer that compares priorities (i.e. <see langword="sbyte"/> values) in descending order, so that higher
/// priorities are first.
/// </summary>
public class PriorityComparer : IComparer<sbyte>
{
    /// <summary>
    /// A static instance of this comparer.
    /// </summary>
    public static readonly PriorityComparer Default = new();

    /// <summary>
    /// A static instance of this comparer with the order inverted, so that lower priorities are first.
    /// </summary>
    public static readonly PriorityComparer Inverted = new() { IsInverted = true };

    /// <summary>
    /// Constructs a new priority comparer.
    /// </summary>
    /// <remarks>
    /// This constructor is protected to prevent external instantiation, as this class is intended to be used via the
    /// static instances <see cref="Default"/> and <see cref="Inverted"/>, but may still be inherited from.
    /// </remarks>
    protected PriorityComparer() { }

    /// <summary>
    /// Inverts this comparer, so that lower priorities are first.
    /// </summary>
    public bool IsInverted { get; init; }

    /// <inheritdoc/>
    public int Compare(sbyte x, sbyte y)
    {
        if (IsInverted)
        {
            return x.CompareTo(y);
        }
        else
        {
            return y.CompareTo(x);
        }
    }
}
