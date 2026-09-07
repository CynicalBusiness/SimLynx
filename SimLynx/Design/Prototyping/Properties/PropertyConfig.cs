using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Core;
using SimLynx.Design.Prototyping.Blueprints;

namespace SimLynx.Design.Prototyping.Properties;

/// <summary>
/// Static helper methods for creating prototype property configs.
/// </summary>
public static class PropertyConfig
{
    private static readonly MethodInfo genericCreateMethod = typeof(PropertyConfig).GetMethod(
        nameof(Create),
        2,
        BindingFlags.Public | BindingFlags.Static,
        null,
        [typeof(PropertyInfo)],
        null
    )!;

    /// <summary>
    /// Creates a new prototype property config for the given <paramref name="property"/> of the subject type
    /// <typeparamref name="TSubject"/>.
    /// </summary>
    /// <typeparam name="TSubject">The type of the subject for which this config is being applied.</typeparam>
    /// <typeparam name="TValue">The type of the property.</typeparam>
    /// <param name="property">The property info for the prototype property.</param>
    /// <returns>The created prototype property config.</returns>
    public static IPropertyConfig<TSubject> Create<TSubject, TValue>(PropertyInfo property)
        where TSubject : class, IPrototypeSubject
    {
        return new PropertyConfig<TSubject, TValue>(property);
    }

    /// <summary>
    /// Creates a new prototype property config for the given <paramref name="property"/> of the subject type
    /// <typeparamref name="TSubject"/>.
    /// </summary>
    /// <typeparam name="TSubject">The type of the subject for which this config is being applied.</typeparam>
    /// <param name="property">The property info for the prototype property.</param>
    /// <returns>The created prototype property config.</returns>
    public static IPropertyConfig<TSubject> Create<TSubject>(PropertyInfo property)
        where TSubject : class, IPrototypeSubject
    {
        return (IPropertyConfig<TSubject>)
            genericCreateMethod.MakeGenericMethod(typeof(TSubject), property.PropertyType).Invoke(null, [property])!;
    }
}

/// <summary>
/// Prototype config for a class property of a prototype-able <typeparamref name="TValue"/> property of the subject.
/// </summary>
/// <typeparam name="TSubject">The type of the subject for which this config is being applied.</typeparam>
/// <typeparam name="TValue">The type value of the property</typeparam>
/// <param name="property">The property info for the prototype property.</param>
public class PropertyConfig<TSubject, TValue>(PropertyInfo property)
    : PrototypeValueConfig<TSubject, TValue>(property.Name),
        IPropertyConfig<TSubject, TValue>
    where TSubject : class, IPrototypeSubject
{
    /// <inheritdoc/>
    public PropertyInfo Property { get; } = property;

    /// <inheritdoc/>
    public override bool Apply(IBlueprintBuilder<TSubject> builder)
    {
        if (!TryCreateProvider(out var valueFunc))
        {
            return false;
        }

        var subjectParamExpr = Expression.Parameter(typeof(TSubject), "subject");
        var propertyExpr = Expression.Property(subjectParamExpr, Property);
        if (Property.IsNativeRequired)
        {
            // Autofac tries to handle "native required" properties, so we inject the default
            // so Autofac doesn't complain about that property (since we're handling it)
            builder.InjectionParameters.Add(new NamedPropertyParameter(Property.Name, default!));
        }

        var assignExpr = Expression.Assign(propertyExpr, Expression.Invoke(Expression.Constant(valueFunc)));

        Expression configureExpr = HasConfigurations
            ? Expression.Block(
                Configurations
                    .Select(c =>
                        Expression.Assign(propertyExpr, Expression.Invoke(Expression.Constant(c), propertyExpr))
                    )
                    .Prepend(assignExpr)
            )
            : assignExpr;

        var configureAction = Expression.Lambda<Action<TSubject>>(configureExpr, subjectParamExpr).Compile();
        builder.ConfigureAfterCreate((context, subject) => configureAction.Invoke(subject));

        return true;
    }

    /// <inheritdoc/>
    public override void Validate()
    {
        base.Validate();

        if (Property.IsRequired && !HasValue)
        {
            throw new InvalidOperationException(
                $"Property '{Property.Name}' of type '{typeof(TSubject).FullName}' is required and must be configured."
            );
        }
    }

    /// <inheritdoc/>
    protected override bool TryGetDefaultProvider([MaybeNullWhen(false)] out Func<TValue> provider)
    {
        if (Property.IsRequired)
        {
            // required properties *must* explicitly provide a value
            provider = null;
            return false;
        }

        return base.TryGetDefaultProvider(out provider);
    }

    /// <inheritdoc/>
    protected override IEnumerable<Expression> GetConfigurationExpressions(ParameterExpression valueExpr)
    {
        // we want to round-trip through the property, so don't apply them here
        return [];
    }
}
