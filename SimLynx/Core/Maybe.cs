using System;
using System.Diagnostics.CodeAnalysis;

namespace SimLynx.Core;

/// <summary>
/// Container type for an optional <typeparamref name="T"/> value. Similar to <see cref="Nullable{T}"/>, but can be used
/// with reference types where a distinction between "no value" and "value but null" is desired.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct Maybe<T>
{
    /// <summary>
    /// A <see cref="Maybe{T}"/> instance with no value.
    /// </summary>
    public static readonly Maybe<T> None = new();

    /// <summary>
    /// Indicates whether the <see cref="Maybe{T}"/> instance has a value.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Indicates whether this instance has a <see cref="Value"/> and it is non-null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasNonNullValue => HasValue && Value is not null;

    /// <summary>
    /// The current value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Constructs a new instance with no value.
    /// </summary>
    public Maybe()
    {
        HasValue = false;
        Value = default!;
    }

    /// <summary>
    /// Constructs a new instance with the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to construct with.</param>
    public Maybe(T value)
    {
        HasValue = true;
        Value = value;
    }
}
