using System;
using System.Reflection;
using Autofac;
using Autofac.Core;

namespace SimLynx.Core;

/// <summary>
/// A parameter that works similarly to Autofac's <see cref="TypedParameter"/> but matches a parameter that the provided
/// value is assignable to, rather than requiring an exact type match.
/// </summary>
public class LooseTypedParameter : ConstantParameter
{
    private static Predicate<ParameterInfo> CreatePredicate(Type valueType, Type? ceilingType)
    {
        bool valuePredicate(ParameterInfo pi) => pi.ParameterType.IsAssignableFrom(valueType);

        if (ceilingType is null)
        {
            return valuePredicate;
        }
        else if (ceilingType.IsGenericTypeDefinition)
        {
            return pi => pi.ParameterType.IsGenericTypeOf(ceilingType) && valuePredicate(pi);
        }
        else
        {
            return pi => pi.ParameterType.IsAssignableTo(ceilingType) && valuePredicate(pi);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LooseTypedParameter"/> class with the specified value, value type,
    /// and optional ceiling type.
    /// </summary>
    /// <param name="valueType">The type of the value.</param>
    /// <param name="value">The value to provide for the parameter.</param>
    /// <param name="ceilingType">The ceiling type to match against. The parameter will only match if it is also assignable to this type.</param>
    /// <exception cref="ArgumentException">If <paramref name="value"/>'s type is not assignable to <paramref name="valueType"/> or if <paramref name="valueType"/> is not assignable to <paramref name="ceilingType"/>.</exception>
    public LooseTypedParameter(Type valueType, object? value, Type? ceilingType = null)
        : base(value, CreatePredicate(valueType, ceilingType))
    {
        if (value is not null && !value.GetType().IsAssignableTo(valueType))
        {
            throw new ArgumentException(
                $"The value {value} is not assignable to the value type {valueType}.",
                nameof(value)
            );
        }

        if (ceilingType is not null && !valueType.IsAssignableTo(ceilingType))
        {
            throw new ArgumentException(
                $"The value type {valueType} is not assignable to the ceiling type {ceilingType}.",
                nameof(value)
            );
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LooseTypedParameter"/> class with the specified value and optional
    /// ceiling type. The value type is inferred from the type of the provided value, which may not be <c>null</c>.
    /// </summary>
    /// <param name="value">The value to provide for the parameter.</param>
    /// <param name="ceilingType">The ceiling type to match against. The parameter will only match if it is also assignable to this type.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
    public LooseTypedParameter(object value, Type? ceilingType = null)
        : this(value is null ? throw new ArgumentNullException(nameof(value)) : value.GetType(), value, ceilingType) { }
}
