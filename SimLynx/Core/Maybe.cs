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
    /// Implicitly converts a <typeparamref name="T"/> value to a <see cref="Maybe{T}"/> instance.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Maybe<T>(T value) => new(value);

    /// <summary>
    /// Implicitly converts a <see cref="Maybe{T}"/> instance to a <typeparamref name="T"/> value.
    /// </summary>
    /// <remarks>
    /// If the <see cref="Maybe{T}"/> instance has no value, the default value of <typeparamref name="T"/> will be
    /// returned.
    /// </remarks>
    /// <param name="maybe">The <see cref="Maybe{T}"/> instance to convert.</param>
    public static implicit operator T(Maybe<T> maybe)
    {
        if (!maybe.HasValue)
        {
            return default!;
        }
        return maybe.Value;
    }

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

    /// <summary>
    /// Returns <see cref="None"/> of <typeparamref name="T"/>.
    /// </summary>
    /// <returns>The no-value instance of <typeparamref name="T"/>.</returns>
    public Maybe<T> AsEmpty()
    {
        return None;
    }

    /// <summary>
    /// Attempts to cast the value of this <typeparamref name="T"/> instance to one of <typeparamref name="U"/>. If this
    /// instance holds no value, an empty instance of the target type is returned.
    /// </summary>
    /// <typeparam name="U">The target type to cast to.</typeparam>
    /// <returns>A <see cref="Maybe{U}"/> instance containing the casted value or empty if no value is present.</returns>
    /// <exception cref="InvalidCastException">Thrown if the value of this instance is not of type <typeparamref name="U"/>.</exception>
    public Maybe<U> Cast<U>()
    {
        if (!HasValue)
        {
            return Maybe<U>.None;
        }

        if (Value is null)
        {
            return new Maybe<U>(default!);
        }

        if (Value is not U castedValue)
        {
            throw new InvalidCastException($"Cannot cast value of type {typeof(T)} to {typeof(U)}.");
        }

        return new Maybe<U>(castedValue);
    }
}
