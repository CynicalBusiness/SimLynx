using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimLynx.Core.Prototyping.Properties;

/// <summary>
/// Prototype config for a class property of a prototype-able <typeparamref name="TValue"/> property of the subject.
/// </summary>
/// <typeparam name="TValue">The type value of the property</typeparam>
/// <param name="resolver">The resolver for the prototype config.</param>
/// <param name="property">The property info for the prototype property.</param>
public class PrototypePropertyConfig<TValue>(IPrototypeConfigResolver resolver, PropertyInfo property)
    : IPrototypePropertyConfig
{
    /// <inheritdoc/>
    public PropertyInfo Property { get; } = property;

    /// <inheritdoc/>
    public string Name => Property.Name;

    /// <inheritdoc/>
    public IPrototypeConfigResolver Resolver { get; } = resolver;

    /// <inheritdoc cref="IPrototypePropertyConfig.Value"/>
    public Maybe<TValue> Value
    {
        get => field;
        set
        {
            field = value;
            if (value.HasValue)
            {
                // Clear modifiers when a value is set, as modifiers applied before this value are no longer relevant.
                // we only apply modifiers which come after the most recently set value.
                Modifiers.Clear();
            }
        }
    } = Maybe<TValue>.None;

    /// <inheritdoc cref="IPrototypePropertyConfig.Modifiers"/>
    public List<Func<TValue, TValue>> Modifiers { get; } = [];

    /// <inheritdoc/>
    public bool HasModifiers => Modifiers.Count > 0;

    IEnumerable<Func<object?, object?>> IPrototypePropertyConfig.Modifiers
    {
        get
        {
            foreach (var modifier in Modifiers)
            {
                yield return value => modifier((TValue)value!);
            }
        }
    }

    Maybe<object?> IPrototypePropertyConfig.Value
    {
        get => Value.Cast<object?>();
        set => Value = value.Cast<TValue>();
    }

    /// <inheritdoc/>
    public bool Clear()
    {
        if (Value.HasValue || HasModifiers)
        {
            Value = Maybe<TValue>.None;
            Modifiers.Clear();
            return true;
        }

        return false;
    }

    /// <inheritdoc cref="IPrototypePropertyConfig.ModifyValue(Func{object?, object?})"/>
    public void ModifyValue(Func<TValue, TValue> modifier)
    {
        Modifiers.Add(modifier);
    }

    /// <inheritdoc/>
    void IPrototypePropertyConfig.ModifyValue(Func<object?, object?> modifier)
    {
        Modifiers.Add(value => (TValue)modifier(value)!);
    }
}
