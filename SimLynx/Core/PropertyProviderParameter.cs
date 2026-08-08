using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Autofac;

namespace SimLynx.Core;

/// <summary>
/// A parameter which acts as a factory for property values. The factory is invoked for each injection.
/// </summary>
/// <param name="selector">The property selector used to determine which properties to inject.</param>
/// <param name="valueFactory">The factory function used to create property values.</param>
public class PropertyProviderParameter(
    PropertyProviderParameter.ParameterSelector selector,
    PropertyProviderParameter.ValueFactory valueFactory
) : PropertyParameter()
{
    /// <summary>
    /// Selector used to determine if the parameter can supply a value for a given <see cref="PropertyInfo"/>.
    /// </summary>
    /// <param name="propertyInfo">The property info to check.</param>
    /// <returns><c>true</c> if the parameter can supply a value; otherwise, <c>false</c>.</returns>
    public delegate bool ParameterSelector(PropertyInfo propertyInfo);

    /// <summary>
    /// Factory function used to produce a value for a given <see cref="PropertyInfo"/> and <see cref="IComponentContext"/>.
    /// </summary>
    /// <param name="propertyInfo">The property info for which to produce a value.</param>
    /// <param name="context">The component context.</param>
    /// <returns>The produced value.</returns>
    public delegate object? ValueFactory(PropertyInfo propertyInfo, IComponentContext context);

    /// <summary>
    /// The factory function used to create parameter values.
    /// </summary>
    public ValueFactory Factory { get; } = valueFactory;

    /// <summary>
    /// The selector used to determine if the parameter can supply a value for a given <see cref="PropertyInfo"/>.
    /// </summary>
    public ParameterSelector Selector { get; } = selector;

    /// <inheritdoc/>
    public override bool CanSupplyValue(
        PropertyInfo propertyInfo,
        IComponentContext context,
        [NotNullWhen(true)] out Func<object?>? valueProvider
    )
    {
        if (Selector(propertyInfo))
        {
            valueProvider = () => Factory(propertyInfo, context);
            return true;
        }
        valueProvider = null;
        return false;
    }
}
