using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Autofac;
using Autofac.Core;

namespace SimLynx.Core;

/// <summary>
/// A type of injection parameter which can supply values for properties.
/// </summary>
public abstract class PropertyParameter : Parameter
{
    /// <inheritdoc/>
    public override bool CanSupplyValue(
        ParameterInfo pi,
        IComponentContext context,
        [NotNullWhen(true)] out Func<object?>? valueProvider
    )
    {
        if (!pi.TryGetDeclaringProperty(out var prop))
        {
            valueProvider = null;
            return false;
        }

        return CanSupplyValue(prop, context, out valueProvider);
    }

    /// <summary>
    /// Determines if the parameter can supply a value for the given <paramref name="propertyInfo"/>.
    /// </summary>
    /// <param name="propertyInfo">The property for which the value is being supplied.</param>
    /// <param name="context">The component context.</param>
    /// <param name="valueProvider">The value provider delegate.</param>
    /// <returns>True if the parameter can supply a value for the given <paramref name="propertyInfo"/>; otherwise, false.</returns>
    public abstract bool CanSupplyValue(
        PropertyInfo propertyInfo,
        IComponentContext context,
        [NotNullWhen(true)] out Func<object?>? valueProvider
    );
}
