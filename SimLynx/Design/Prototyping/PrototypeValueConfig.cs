using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// Base class for prototype configs that configure a value.
/// </summary>
/// <typeparam name="TSubject"></typeparam>
/// <typeparam name="TValue"></typeparam>
public abstract class PrototypeValueConfig<TSubject, TValue>(Identifier name)
    : PrototypeConfig<TSubject>(name),
        IPrototypeValueConfig<TSubject, TValue>
    where TSubject : class, IPrototypeSubject
{
    /// <summary>
    /// The configurations to apply to the value created by this configuration.
    /// </summary>
    public ImmutableList<Func<TValue, TValue>> Configurations { get; private set; } = [];

    /// <inheritdoc/>
    public virtual Func<TValue>? ValueFactory
    {
        get => field;
        set
        {
            AttributeCurrent();
            field = value;
        }
    }

    /// <inheritdoc/>
    public virtual Type ValueType => typeof(TValue);

    /// <inheritdoc/>
    public bool HasValue => ValueFactory is not null;

    /// <inheritdoc/>
    public bool HasConfigurations => Configurations.Count > 0;

    /// <inheritdoc/>
    public override bool IsEmpty => base.IsEmpty && !HasValue && !HasConfigurations;

    /// <inheritdoc/>
    public override void Clear()
    {
        base.Clear();
        ValueFactory = null;
        Configurations = [];
    }

    /// <inheritdoc/>
    public void Configure(Func<TValue> valueFunc)
    {
        ValueFactory = valueFunc;
    }

    /// <inheritdoc/>
    public virtual void Configure(Func<TValue, TValue> configurationFunc)
    {
        Configurations = Configurations.Add(configurationFunc);
        AttributeCurrent();
    }

    /// <inheritdoc/>
    public void Configure(Action<TValue> configuration)
    {
        Configure(
            (value) =>
            {
                configuration(value);
                return value;
            }
        );
    }

    /// <inheritdoc/>
    public override void CopyTo(IPrototypeConfig other)
    {
        base.CopyTo(other);

        if (other is IPrototypeValueConfigOf<TValue> target)
        {
            if (ValueFactory is not null)
            {
                target.ValueFactory = ValueFactory;
            }

            foreach (var configuration in Configurations)
            {
                target.Configure(configuration);
            }
        }
    }

    /// <inheritdoc/>
    public virtual bool TryCreateProvider([MaybeNullWhen(false)] out Func<TValue> provider)
    {
        var factory = ValueFactory;
        if (factory is null && !TryGetDefaultProvider(out factory))
        {
            provider = null;
            return false;
        }

        var valueExpr = Expression.Variable(typeof(TValue), "value");
        var factoryExpr = Expression.Invoke(Expression.Constant(factory));
        var blockExpr = Expression.Block(
            [valueExpr],
            GetConfigurationExpressions(valueExpr)
                .Prepend(Expression.Assign(valueExpr, Expression.Invoke(factoryExpr)))
                .Append(valueExpr)
        );

        provider = Expression.Lambda<Func<TValue>>(blockExpr).Compile();
        return true;
    }

    /// <inheritdoc/>
    public virtual bool TryCreateValue([MaybeNullWhen(false)] out TValue value)
    {
        if (TryCreateProvider(out var provider))
        {
            value = provider();
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Attempts to get a default value provider for the configured value type.
    /// </summary>
    /// <param name="provider">The default value provider if available; otherwise, null.</param>
    /// <returns>True if a default value provider is available; otherwise, false.</returns>
    protected virtual bool TryGetDefaultProvider([MaybeNullWhen(false)] out Func<TValue> provider)
    {
        if (typeof(TValue).IsValueType || typeof(TValue).IsDeclaredNullable)
        {
            provider = static () => default!;
            return true;
        }

        provider = null;
        return false;
    }

    /// <summary>
    /// Gets an enumeration of expressions to apply the configurations to the value created by this configuration in
    /// the value provider.
    /// </summary>
    /// <remarks>
    /// To apply configurations in a custom way or during another step, an empty enumeration can be returned.
    /// </remarks>
    /// <param name="valueExpr">The expression representing the value to be configured.</param>
    /// <returns>An enumeration of expressions to apply the configurations.</returns>
    protected virtual IEnumerable<Expression> GetConfigurationExpressions(ParameterExpression valueExpr)
    {
        return Configurations.Select(c => Expression.Invoke(Expression.Constant(c), valueExpr));
    }
}
