using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace SimLynx.Design.Prototyping.Properties;

/// <summary>
/// Extension methods for prototyping properties.
/// </summary>
public static class PrototypePropertyExtensions
{
    extension<TSubject>(IPrototype<TSubject> @this)
        where TSubject : class, IPrototypeSubject
    {
        /// <summary>
        /// Gets the configuration slot for properties of this prototype's subject.
        /// </summary>
        public PropertyConfigSlot<TSubject> Properties
        {
            get
            {
                if (
                    !@this.TryGetSlot<IPropertyConfig<TSubject>>(out var rawSlot)
                    || rawSlot is not PropertyConfigSlot<TSubject> slot
                )
                {
                    throw new InvalidOperationException("Properties not valid on this prototype");
                }
                return slot;
            }
        }

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
                ?? throw new ArgumentException(
                    $"The property '{propertyName}' on {@this.SubjectType} (for prototype {@this}) is not configurable.",
                    nameof(propertyName)
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
                ?? throw new ArgumentException(
                    $"The property '{propertyInfo.Name}' on {@this.SubjectType} (for prototype {@this}) is not configurable.",
                    nameof(propertySelector)
                );
        }
    }

    extension<TSubject>(IPrototypeConfigSlotOf<IPropertyConfig<TSubject>> @this)
        where TSubject : class, IPrototypeSubject
    {
        /// <summary>
        /// Gets a property config for the property specified by the given <paramref name="propertySelector"/>. Returns
        /// null if the property does not exist or is not configurable.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="propertySelector">An expression representing the property.</param>
        /// <returns>The property configuration, if found; otherwise, null.</returns>
        public IPropertyConfig<TSubject, TValue>? Get<TValue>(Expression<Func<TSubject, TValue>> propertySelector)
        {
            var prop = propertySelector.GetSelectedProperty();
            return @this.Get(prop.Name) as IPropertyConfig<TSubject, TValue>;
        }
    }

    extension(PropertyInfo @this)
    {
        /// <summary>
        /// Indicates whether this property is configurable, meaning it can be set by a prototype.
        /// </summary>
        /// <remarks>
        /// A property is considered configurable if it has a public setter, or the behavior is explicitly defined
        /// using the <see cref="ConfigurableAttribute"/>.
        /// </remarks>
        public bool IsPrototypeConfigurable
        {
            get
            {
                var setMethod = @this.SetMethod;
                if (setMethod is null)
                {
                    return false;
                }

                var configurableAttribute = @this.GetCustomAttribute<ConfigurableAttribute>(inherit: true);
                return configurableAttribute is null ? setMethod.IsPublic : configurableAttribute.IsConfigurable;
            }
        }
    }
}
