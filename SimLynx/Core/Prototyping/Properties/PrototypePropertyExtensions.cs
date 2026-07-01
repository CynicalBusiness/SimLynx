using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SimLynx.Core.Prototyping.Properties;

/// <summary>
/// Extension methods for prototyping properties.
/// </summary>
public static class PrototypePropertyExtensions
{
    extension(IPrototype @this)
    {
        /// <summary>
        /// Helper to get a property config for a given <paramref name="propertyName"/>. Throws an exception if the
        /// property does not exist or is not configurable.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The property configuration.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the property does not exist or is not configurable.</exception>
        public IPrototypePropertyConfig GetPropertyConfig(string propertyName)
        {
            if (
                !@this.TryGetConfig<IPrototypePropertyConfig>(
                    PrototypePropertyConfigResolver.PROPERTIES_SLOT_NAME,
                    propertyName,
                    out var config
                )
            )
            {
                throw new InvalidOperationException(
                    $"No valid configurable property '{propertyName}' found on {@this.SubjectType} (for prototype {@this.Id})."
                );
            }

            return config;
        }

        /// <summary>
        /// Helper to set a property value for a given <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The prototype instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the property does not exist or is not configurable.</exception>
        public IPrototype SetProperty(string propertyName, object? value)
        {
            var config = @this.GetPropertyConfig(propertyName);
            config.Value = value;
            return @this;
        }

        /// <summary>
        /// Helper to clear a property value for a given <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The prototype instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the property does not exist or is not configurable.</exception>
        public IPrototype ClearProperty(string propertyName)
        {
            var config = @this.GetPropertyConfig(propertyName);
            config.Clear();
            return @this;
        }

        /// <summary>
        /// Helper to add a modifier to a property for a given <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="modifier">The modifier to add.</param>
        /// <returns>The prototype instance.</returns>
        public IPrototype ModifyProperty(string propertyName, Func<object?, object?> modifier)
        {
            var config = @this.GetPropertyConfig(propertyName);
            config.ModifyValue(modifier);
            return @this;
        }
    }

    extension<TSubject>(IPrototype<TSubject> @this)
        where TSubject : class, IPrototypeSubject
    {
        /// <summary>
        /// Helper to get a <typeparamref name="TValue"/> property config for the property specified by the given
        /// <paramref name="propertyExpression"/>. Throws an exception if the property does not exist or is
        /// not configurable.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="propertyExpression">An expression representing the property.</param>
        /// <returns>The property configuration.</returns>
        /// <exception cref="ArgumentException">Thrown if the expression is not a valid property expression.</exception>
        public PrototypePropertyConfig<TValue> GetPropertyConfig<TValue>(
            Expression<Func<TSubject, TValue>> propertyExpression
        )
        {
            if (
                propertyExpression.Body is not MemberExpression memberExpression
                || memberExpression.Member is not PropertyInfo propertyInfo
            )
            {
                throw new ArgumentException(
                    $"The given expression '{propertyExpression}' is not a valid property expression.",
                    nameof(propertyExpression)
                );
            }

            var config = @this.GetPropertyConfig(propertyInfo.Name);
            if (config is not PrototypePropertyConfig<TValue> typedConfig)
            {
                throw new InvalidOperationException(
                    $"The property '{propertyInfo.Name}' on {@this.SubjectType} (for prototype {@this.Id}) is not of type {typeof(TValue)}."
                );
            }

            return typedConfig;
        }

        /// <summary>
        /// Helper to set a <typeparamref name="TValue"/> property value for the property specified by the given
        /// <paramref name="propertyExpression"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="propertyExpression">An expression representing the property.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The prototype instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the property does not exist or is not configurable.</exception>
        public IPrototype<TSubject> SetProperty<TValue>(
            Expression<Func<TSubject, TValue>> propertyExpression,
            TValue value
        )
        {
            var config = @this.GetPropertyConfig(propertyExpression);
            config.Value = value;
            return @this;
        }

        /// <summary>
        /// Helper to clear a <typeparamref name="TValue"/> property value for the property specified by the given
        /// <paramref name="propertyExpression"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="propertyExpression">An expression representing the property.</param>
        /// <returns>The prototype instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the property does not exist or is not configurable.</exception>
        public IPrototype<TSubject> ClearProperty<TValue>(Expression<Func<TSubject, TValue>> propertyExpression)
        {
            var config = @this.GetPropertyConfig(propertyExpression);
            config.Clear();
            return @this;
        }

        /// <summary>
        /// Helper to add a modifier to a <typeparamref name="TValue"/> property for the property specified by the
        /// given <paramref name="propertyExpression"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="propertyExpression">An expression representing the property.</param>
        /// <param name="modifier">The modifier to add.</param>
        /// <returns>The prototype instance.</returns>
        public IPrototype<TSubject> ModifyProperty<TValue>(
            Expression<Func<TSubject, TValue>> propertyExpression,
            Func<TValue?, TValue> modifier
        )
        {
            var config = @this.GetPropertyConfig(propertyExpression);
            config.ModifyValue(modifier);
            return @this;
        }
    }
}
