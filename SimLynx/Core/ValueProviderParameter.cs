using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Autofac;
using Autofac.Core;

namespace SimLynx.Core;

/// <summary>
/// Parameter which acts as a provider for values. The provider is invoked for each injection.
/// </summary>
/// <param name="selector">A function which determines if the parameter can supply a value for a given <see cref="ParameterInfo"/>.</param>
/// <param name="valueFactory">A function which produces a value for a given <see cref="ParameterInfo"/> and <see cref="IComponentContext"/>.</param>
public class ValueProviderParameter(
    ValueProviderParameter.ParameterSelector selector,
    ValueProviderParameter.ValueFactory valueFactory
) : Parameter
{
    /// <summary>
    /// Selector used to determine if the parameter can supply a value for a given <see cref="ParameterInfo"/>.
    /// </summary>
    /// <param name="parameterInfo">The parameter info to check.</param>
    /// <returns><c>true</c> if the parameter can supply a value; otherwise, <c>false</c>.</returns>
    public delegate bool ParameterSelector(ParameterInfo parameterInfo);

    /// <summary>
    /// Factory function used to produce a value for a given <see cref="ParameterInfo"/> and <see cref="IComponentContext"/>.
    /// </summary>
    /// <param name="parameterInfo">The parameter info for which to produce a value.</param>
    /// <param name="context">The component context.</param>
    /// <returns>The produced value.</returns>
    public delegate object? ValueFactory(ParameterInfo parameterInfo, IComponentContext context);

    /// <summary>
    /// The factory function used to create parameter values.
    /// </summary>
    public ValueFactory Factory { get; } = valueFactory;

    /// <summary>
    /// The selector used to determine if the parameter can supply a value for a given <see cref="ParameterInfo"/>.
    /// </summary>
    public ParameterSelector Selector { get; } = selector;

    /// <inheritdoc/>
    public override bool CanSupplyValue(
        ParameterInfo pi,
        IComponentContext context,
        [NotNullWhen(true)] out Func<object?>? valueProvider
    )
    {
        if (Selector(pi))
        {
            valueProvider = () => Factory(pi, context);
            return true;
        }

        valueProvider = null;
        return false;
    }
}
