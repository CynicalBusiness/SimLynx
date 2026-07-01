using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SimLynx.Core.Prototyping.Blueprints;

namespace SimLynx.Core.Prototyping.Properties;

/// <summary>
/// Prototype config resolver for <see cref="IPrototypePropertyConfig"/>.
/// </summary>
public class PrototypePropertyConfigResolver : IPrototypeConfigResolver<IPrototypePropertyConfig>
{
    private readonly MethodInfo typedExpressionMethodDef = typeof(PrototypePropertyConfigResolver).GetMethod(
        nameof(BuildValueExpressions),
        BindingFlags.NonPublic | BindingFlags.Instance
    )!;

    /// <summary>
    /// The slot for prototype property configs.
    /// </summary>
    public const string PROPERTIES_SLOT_NAME = "Properties";

    /// <inheritdoc/>
    public required Symbol Slot { get; init; }

    /// <inheritdoc/>
    public IPrototypePropertyConfig? TryCreate<TSubject>(IPrototype<TSubject> prototype, string name)
        where TSubject : class, IPrototypeSubject
    {
        var typeInfo = PrototypePropertyTypeInfo.For<TSubject>();

        if (!typeInfo.TryGetProperty(name, out var property))
        {
            return null;
        }

        return (IPrototypePropertyConfig)
            Activator.CreateInstance(
                typeof(PrototypePropertyConfig<>).MakeGenericType(property.PropertyType),
                this,
                property
            )!;
    }

    /// <inheritdoc/>
    public void Compile<TSubject>(
        IEnumerable<IPrototypePropertyConfig> configs,
        BlueprintBuilder<TSubject> blueprintBuilder
    )
        where TSubject : class, IPrototypeSubject
    {
        var effectiveConfigs = new Dictionary<string, IPrototypePropertyConfig>();
        foreach (var requiredProp in PrototypePropertyTypeInfo.For<TSubject>().Properties.Where(p => p.IsRequired))
        {
            var config = TryCreate(blueprintBuilder.Prototype, requiredProp.Name);
            if (config is not null)
            {
                effectiveConfigs[requiredProp.Name] = config;
            }
        }

        foreach (var config in configs)
        {
            if (!effectiveConfigs.TryGetValue(config.Name, out var effectiveConfig))
            {
                effectiveConfig = TryCreate(blueprintBuilder.Prototype, config.Name);
                if (effectiveConfig is null)
                {
                    continue;
                }

                effectiveConfigs[config.Name] = effectiveConfig;
            }

            if (config.Value.HasValue)
            {
                // clears any modifiers applied before this, since they are no longer relevant
                effectiveConfig.Value = config.Value;
            }

            foreach (var modifier in config.Modifiers)
            {
                effectiveConfig.ModifyValue(modifier);
            }
        }

        var unconfiguredRequiredProps = effectiveConfigs
            .Values.Where(c => c.Property.IsRequired && !c.Value.HasValue)
            .ToList();
        if (unconfiguredRequiredProps.Count > 0)
        {
            throw new AggregateException(
                $"Required properties of type {typeof(TSubject)} for {blueprintBuilder.Prototype} are missing values.",
                unconfiguredRequiredProps.Select(c => new InvalidOperationException(
                    $"Required property {typeof(TSubject)}#{c.Name} must have a value defined for prototype '{blueprintBuilder.Prototype}'."
                ))
            );
        }

        var subjectParamExpr = Expression.Parameter(typeof(TSubject), "subject");
        var blockExpr = Expression.Block(
            effectiveConfigs
                .Values.Where(c => c.IsConfigured)
                .Select(c => BuildExpression<TSubject>(c, subjectParamExpr))
                .Where(expr => expr is not null)!
        );
        var lambdaExpr = Expression.Lambda<Action<TSubject>>(blockExpr, subjectParamExpr);

        blueprintBuilder.OnCreated += lambdaExpr.Compile();
    }

    /// <summary>
    /// Gets an expression that applies a prototype property config to a subject.
    /// </summary>
    /// <typeparam name="TSubject">The type of the subject.</typeparam>
    /// <param name="config">The prototype property config to compile.</param>
    /// <param name="subjectParamExpr">The parameter expression representing the subject.</param>
    /// <returns>An expression that applies the config to a subject.</returns>
    /// <exception cref="InvalidOperationException">If the config is of an unexpected type.</exception>
    protected virtual Expression? BuildExpression<TSubject>(
        IPrototypePropertyConfig config,
        ParameterExpression subjectParamExpr
    )
        where TSubject : class, IPrototypeSubject
    {
        var configType = config.GetType();
        if (!configType.IsSubclassOfGenericDefinition(typeof(PrototypePropertyConfig<>), out var genericDef))
        {
            throw new InvalidOperationException(
                $"Unexpected config: config type {configType} is not a subclass of PrototypePropertyConfig<>."
            );
        }

        var valueType = genericDef.GetGenericArguments()[0];
        var exprMethod = typedExpressionMethodDef.MakeGenericMethod(typeof(TSubject), valueType);

        if (exprMethod.Invoke(null, [config, subjectParamExpr]) is not IEnumerable<Expression> exprs)
        {
            return null;
        }

        var block = Expression.Block(exprs);
        return block.Expressions.Count > 0 ? block : null;
    }

    /// <summary>
    /// Gets an expression that applies a prototype property config to a subject.
    /// </summary>
    /// <typeparam name="TSubject">The type of the subject.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="config">The prototype property config to compile.</param>
    /// <param name="subjectParamExpr">The parameter expression representing the subject.</param>
    /// <returns>An action that applies the config to a subject.</returns>
    protected virtual IEnumerable<Expression> BuildValueExpressions<TSubject, TValue>(
        PrototypePropertyConfig<TValue> config,
        ParameterExpression subjectParamExpr
    )
        where TSubject : class, IPrototypeSubject
    {
        if (config.Property.PropertyType != typeof(TValue))
        {
            throw new InvalidOperationException(
                $"Unexpected config: config property type {config.Property.PropertyType} does not match expected value type: {typeof(TValue)}."
            );
        }
        var propertyExpr = Expression.Property(subjectParamExpr, config.Property);

        if (config.Value.HasValue)
        {
            var valueExpr = Expression.Constant(config.Value.Value, typeof(TValue));
            yield return Expression.Assign(propertyExpr, valueExpr);
        }

        if (config.HasModifiers)
        {
            foreach (var modifier in config.Modifiers)
            {
                // always round-trip through the property for each modifier so getters/setters are respected
                var modifierExpr = Expression.Constant(modifier, typeof(Func<TValue, TValue>));
                yield return Expression.Assign(propertyExpr, Expression.Invoke(modifierExpr, propertyExpr));
            }
        }
    }
}
