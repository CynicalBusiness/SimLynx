using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace SimLynx.Core.Prototyping.Properties;

/// <summary>
/// Extension methods for prototyping properties.
/// </summary>
public static class PrototypePropertyExtensions
{
    extension<TSubject>(IPrototype<TSubject> @this)
        where TSubject : class, IPrototypeSubject
    {
        /// <summary>
        /// Helper to try to get a property config for a given <paramref name="propertyName"/>. Returns false if the
        /// property does not exist or is not configurable.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="config">The property configuration, if found; otherwise, null.</param>
        /// <returns>True if the property config was found; otherwise, false.</returns>
        public bool TryGetProperty(string propertyName, [MaybeNullWhen(false)] out IPropertyConfig<TSubject> config)
        {
            return @this.TryGetConfig(propertyName, out config);
        }

        /// <summary>
        /// Helper to get a property config for a given <paramref name="propertyName"/>. Throws an exception if the
        /// property does not exist or is not configurable.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The property configuration.</returns>
        public IPropertyConfig<TSubject> GetProperty(string propertyName)
        {
            return @this.GetConfig<IPropertyConfig<TSubject>>(propertyName)
                ?? throw new InvalidOperationException(
                    $"The property '{propertyName}' on {@this.SubjectType} (for prototype {@this}) is not configurable."
                );
        }

        /// <summary>
        /// Helper to try to get a <typeparamref name="TValue"/> property config for the property specified by the given
        /// <paramref name="propertySelector"/>. Returns false if the property does not exist or is not configurable.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="propertySelector">An expression representing the property.</param>
        /// <param name="config">The property configuration, if found; otherwise, null.</param>
        /// <returns>True if the property config was found; otherwise, false.</returns>
        public bool TryGetProperty<TValue>(
            Expression<Func<TSubject, TValue>> propertySelector,
            [MaybeNullWhen(false)] out PropertyConfig<TSubject, TValue> config
        )
        {
            var propertyInfo = propertySelector.GetSelectedProperty();

            return @this.TryGetConfig(propertyInfo.Name, out config);
        }

        /// <summary>
        /// Helper to get a <typeparamref name="TValue"/> property config for the property specified by the given
        /// <paramref name="propertySelector"/>. Throws an exception if the property does not exist or is
        /// not configurable.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="propertySelector   ">An expression representing the property.</param>
        /// <returns>The property configuration.</returns>
        /// <exception cref="ArgumentException">Thrown if the expression is not a valid property expression.</exception>
        public PropertyConfig<TSubject, TValue> GetProperty<TValue>(Expression<Func<TSubject, TValue>> propertySelector)
        {
            var propertyInfo = propertySelector.GetSelectedProperty();

            return @this.GetConfig<IPropertyConfig<TSubject>>(propertyInfo.Name) as PropertyConfig<TSubject, TValue>
                ?? throw new InvalidOperationException(
                    $"The property '{propertyInfo.Name}' on {@this.SubjectType} (for prototype {@this}) is not configurable."
                );
        }
    }
}
