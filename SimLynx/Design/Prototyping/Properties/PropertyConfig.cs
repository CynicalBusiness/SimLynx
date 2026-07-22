using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
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
    : IPropertyConfig<TSubject>,
        IPropertyConfigState<TValue>
    where TSubject : class, IPrototypeSubject
{
    private static readonly Lazy<ValueFunc?> defaultValueFunc = new(() =>
    {
        if (typeof(TValue).IsValueType)
        {
            return static () => default!;
        }
        var ctor = typeof(TValue).GetConstructor([]);
        if (ctor is not null)
        {
            return () => (TValue)ctor.Invoke([]);
        }

        return null;
    });

    /// <summary>
    /// Delegate for a configuration function.
    /// </summary>
    /// <param name="currentValue">The current value, if any.</param>
    /// <returns>The configured value.</returns>
    public delegate TValue ConfigurationFunc(TValue currentValue);

    /// <summary>
    /// Delegate for a value provider function.
    /// </summary>
    /// <returns>The provided value.</returns>
    public delegate TValue ValueFunc();

    private Maybe<ValueFunc> value = Maybe<ValueFunc>.None;
    private readonly List<ConfigurationFunc> configurations = [];

    /// <inheritdoc/>
    public PropertyInfo Property { get; } = property;

    /// <inheritdoc/>
    public string Name => Property.Name;

    /// <inheritdoc/>
    public Type ValueType => typeof(TValue);

    /// <inheritdoc/>
    public bool HasValue => value.HasValue;

    /// <inheritdoc/>
    public bool CanSetValue => HasValue || defaultValueFunc.Value is not null;

    /// <inheritdoc/>
    public bool HasConfigurations => configurations.Count > 0;

    /// <inheritdoc/>
    public bool IsEmpty => !HasValue && !HasConfigurations;

    /// <inheritdoc/>
    public bool Clear()
    {
        if (IsEmpty)
        {
            return false;
        }
        configurations.Clear();
        value = Maybe<ValueFunc>.None;
        return true;
    }

    /// <inheritdoc/>
    public virtual bool Apply(IBlueprintBuilder<TSubject> builder)
    {
        if (IsEmpty)
        {
            return false;
        }

        var didSetValue = false;
        var subjectParamExpr = Expression.Parameter(typeof(TSubject), "subject");
        var propertyExpr = Expression.Property(subjectParamExpr, Property);

        var valueFunc = value.HasValue ? value.Value : defaultValueFunc.Value;
        if (valueFunc is not null)
        {
            if (Property.IsRequired)
            {
                // Autofac is responsible for required property injection, so we add our property via a parameter so that is used instead of resolving from container.
                builder.InjectionParameters.Add(new PropertyValueParameter(Property, valueFunc));
            }
            else
            {
                // For non-required properties Autofac (generally) won't touch, we make an expression ourselves
                // Autofac may still try to resolve the property if some other selector (eg. PropertiesAutowired) is in play, but we have no way of knowing that here, so we're just going to have to step on it.
                var valueProviderExpr = Expression.Invoke(Expression.Constant(valueFunc));
                var assignExpr = Expression.Assign(propertyExpr, valueProviderExpr);
                var assignAction = Expression.Lambda<Action<TSubject>>(assignExpr, subjectParamExpr).Compile();

                builder.ConfigureAfterCreate((context, subject) => assignAction.Invoke(subject));
            }

            didSetValue = true;
        }

        if (configurations.Count > 0)
        {
            // always round-trip the property through the getter/setter to ensure any custom logic is applied.
            var configExprs = configurations.Select(c =>
                Expression.Assign(propertyExpr, Expression.Invoke(Expression.Constant(c), propertyExpr))
            );

            var bodyExpr = Expression.Block(configExprs);
            var configAction = Expression.Lambda<Action<TSubject>>(bodyExpr, subjectParamExpr).Compile();

            builder.ConfigureAfterCreate((context, subject) => configAction.Invoke(subject));
            didSetValue = true;
        }

        return didSetValue;
    }

    /// <summary>
    /// Adds a configuration function to this config, which will be applied in order when the config is applied to a blueprint.
    /// </summary>
    /// <param name="configuration">The configuration to add.</param>
    public void Configure(ConfigurationFunc configuration)
    {
        configurations.Add(configuration);
    }

    /// <summary>
    /// Adds a configuration action to this config, which will be applied in order when the config is applied to a blueprint.
    /// </summary>
    /// <param name="configuration">The configuration action to add.</param>
    public void Configure(Action<TValue> configuration)
    {
        configurations.Add(value =>
        {
            configuration(value);
            return value;
        });
    }

    /// <summary>
    /// Adds a configuration function that sets a new base value for the property when the config is applied to a blueprint.
    /// </summary>
    /// <remarks>
    /// Because this configuration has no dependencies on the current value, all other configurations are cleared
    /// and replaced with this one.
    /// </remarks>
    /// <param name="value"></param>
    public void Configure(ValueFunc value)
    {
        Clear();
        this.value = value;
    }

    /// <inheritdoc cref="IPropertyConfig{TSubject}.CopyTo(IPropertyConfig{TSubject})"/>
    public void CopyTo(PropertyConfig<TSubject, TValue> other)
    {
        CopyStateTo(other);
    }

    private void CopyStateTo(IPropertyConfig other)
    {
        if (other is not IPropertyConfigState<TValue> target)
        {
            throw new ArgumentException(
                $"Cannot copy property config '{Name}' with value type {typeof(TValue)} to a config with value type {other.ValueType}.",
                nameof(other)
            );
        }

        if (value.HasValue)
        {
            var valueFunc = value.Value;
            target.SetValue(() => valueFunc());
        }

        configurations.ForEach(configuration => target.AddConfiguration(value => configuration(value)));
    }

    void IPropertyConfig<TSubject>.CopyTo(IPropertyConfig<TSubject> other) => CopyStateTo(other);

    void IPropertyConfigState.CopyTo(IPropertyConfig other) => CopyStateTo(other);

    void IPropertyConfigState<TValue>.SetValue(Func<TValue> value) => Configure(() => value());

    void IPropertyConfigState<TValue>.AddConfiguration(Func<TValue, TValue> configuration) =>
        Configure(value => configuration(value));

    private class PropertyValueParameter(PropertyInfo property, ValueFunc valueFunc) : Parameter
    {
        public override bool CanSupplyValue(
            ParameterInfo pi,
            IComponentContext context,
            [NotNullWhen(true)] out Func<object?>? valueProvider
        )
        {
            ArgumentNullException.ThrowIfNull(pi, nameof(pi));
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            if (
                !pi.TryGetDeclaringProperty(out var prop)
                || prop.Name != property.Name
                || prop.PropertyType != property.PropertyType
            )
            {
                valueProvider = null;
                return false;
            }

            valueProvider = () => valueFunc();
            return true;
        }
    }
}
