using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using SimLynx.Core;

namespace SimLynx;

/// <summary>
/// Utility extension methods for SimLynx.
/// </summary>
public static class SimLynxExtensions
{
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="this">The source sequence.</param>
    extension<T>(IEnumerable<T> @this)
    {
        /// <summary>
        /// Performs the specified action on each element of the source sequence and yields the element.
        /// </summary>
        /// <param name="action">The action to perform on each element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that yields the elements of the source sequence after performing the action on each element.</returns>
        public IEnumerable<T> Tap(Action<T> action)
        {
            foreach (var item in @this)
            {
                action(item);
                yield return item;
            }
        }

        /// <summary>
        /// Enumerates the source sequence and performs the specified action on each element.
        /// </summary>
        /// <param name="action">The action to perform</param>
        public void ForEach(Action<T, int> action)
        {
            int index = 0;
            foreach (var item in @this)
            {
                action(item, index++);
            }
        }

        /// <inheritdoc cref="ForEach{T}(IEnumerable{T}, Action{T, int})"/>
        public void ForEach(Action<T> action)
        {
            @this.ForEach((item, _) => action(item));
        }
    }

    extension(Type @this)
    {
        /// <summary>
        /// Determines if the specified type is a subclass of a raw generic type.
        /// </summary>
        /// <param name="generic">The generic type to check against.</param>
        /// <param name="bailAtType">The type at which to stop checking the inheritance chain, or <c>null</c> to check all the way up the hierarchy.</param>
        /// <param name="found">The found closed generic type, if any.</param>
        /// <returns><c>true</c> if the type is a subclass of the specified raw generic type; otherwise, <c>false</c>.</returns>
        public bool IsSubclassOfGenericDefinition(Type generic, Type? bailAtType, [MaybeNullWhen(false)] out Type found)
        {
            Type? next = @this;
            while (next != null && next != bailAtType)
            {
                if (
                    (next.IsGenericType && next.GetGenericTypeDefinition() == generic)
                    || (next.IsGenericTypeDefinition && next == generic)
                )
                {
                    found = next;
                    return true;
                }
                next = next.BaseType;
            }
            found = null;
            return false;
        }

        /// <inheritdoc cref="IsSubclassOfGenericDefinition(Type, Type?, out Type)"/>
        public bool IsSubclassOfGenericDefinition(Type generic, [MaybeNullWhen(false)] out Type found)
        {
            return IsSubclassOfGenericDefinition(@this, generic, null, out found);
        }

        /// <inheritdoc cref="IsSubclassOfGenericDefinition(Type, Type?, out Type)"/>
        public bool IsSubclassOfGenericDefinition(Type generic)
        {
            return IsSubclassOfGenericDefinition(@this, generic, null, out _);
        }

        /// <inheritdoc cref="IsSubclassOfGenericDefinition(Type, Type?, out Type)"/>
        public bool IsSubclassOfGenericDefinition(Type generic, Type? bailAtType)
        {
            return IsSubclassOfGenericDefinition(@this, generic, bailAtType, out _);
        }

        /// <summary>
        /// Enumerates the base types of the current class type, starting from but not including the type itself.
        /// </summary>
        /// <remarks>
        /// If the current type is not a class, this method returns an empty sequence.
        /// </remarks>
        /// <returns>An enumeration of the base types of the current class type.</returns>
        public IEnumerable<Type> GetBaseTypes()
        {
            if (!@this.IsClass)
            {
                yield break;
            }

            var next = @this;
            while ((next = next?.BaseType) is not null)
            {
                yield return next;
            }
        }

#if !NET5_0_OR_GREATER // this method is included in .NET 8, use it directly when available
        /// <summary>
        /// Determines whether the current type is assignable to the specified target type, inverse of
        /// <see cref="Type.IsAssignableFrom(Type)"/>.
        /// </summary>
        /// <param name="targetType">The target type to check against.</param>
        /// <returns><c>true</c> if the current type is assignable to the specified target type; otherwise, <c>false</c>.</returns>
        public bool IsAssignableTo(Type targetType)
        {
            return targetType.IsAssignableFrom(@this);
        }
#endif

        /// <inheritdoc cref="CreatePropertySetter{TInstance, TProperty}(PropertyInfo)"/>
        public Action<TInstance, TProperty> CreatePropertySetter<TInstance, TProperty>(
            Expression<Func<TInstance, TProperty>> propertyExpression
        )
        {
            if (propertyExpression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException(
                    "The provided expression does not represent a property access.",
                    nameof(propertyExpression)
                );
            }

            if (memberExpression.Member is not PropertyInfo propertyInfo)
            {
                throw new ArgumentException(
                    "The provided expression does not represent a property access.",
                    nameof(propertyExpression)
                );
            }

            return propertyInfo.CreatePropertySetter<TInstance, TProperty>();
        }
    }

    extension(IMetaType @this)
    {
        /// <summary>
        /// Indicates whether the type represented by this meta-type is abstract.
        /// </summary>
        public bool IsAbstract => @this.Type.IsAbstract;
    }

    extension(MemberInfo member)
    {
        /// <summary>
        /// Indicates whether or not this property is required on its type.
        /// </summary>
        /// <remarks>
        /// A property is considered required if it uses the <see langword="required"/> modifier or marked with
        /// <see cref="RequiredAttribute"/>.
        /// </remarks>
        public bool IsRequired =>
            member.IsDefined(typeof(RequiredAttribute))
            || member.CustomAttributes.Any(attribute =>
                // in case a down-stream consumer uses their own shim, or the compiler includes it itself, look by name
                attribute.AttributeType.FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute"
            );
    }

    extension(PropertyInfo @this)
    {
        /// <summary>
        /// Indicates whether or not this property is init-only, meaning it should only be set during object
        /// initialization.
        /// </summary>
        /// <remarks>
        /// This method only returns <c>true</c> if the property has an "init only" setter. If the setter is not
        /// "init only" or this property <em>has no setter</em>, this method returns <c>false</c>.
        /// <br/>
        /// Init-only setters are a compiler feature; reflection ignores this behavior.
        /// </remarks>
        public bool IsInitOnly =>
            @this.SetMethod is not null
            && @this.SetMethod.ReturnParameter.CustomAttributes.Any(attribute =>
                // in case a down-stream consumer uses their own shim, or the compiler includes it itself, look by name
                attribute.AttributeType.FullName == "System.Runtime.CompilerServices.IsExternalInit"
            );

        /// <summary>
        /// Helper to create a strongly-typed setter delegate for the property, even if the property's setter is
        /// non-public or init-only.
        /// </summary>
        /// <typeparam name="TInstance">The instance type for the setter</typeparam>
        /// <typeparam name="TProperty">The property type for the setter</typeparam>
        /// <returns>A delegate that sets the property value on an instance of <typeparamref name="TInstance"/>.</returns>
        /// <exception cref="ArgumentException">If the instance/property types are invalid</exception>
        public Action<TInstance, TProperty> CreatePropertySetter<TInstance, TProperty>()
        {
            if (!typeof(TProperty).IsAssignableTo(@this.PropertyType))
            {
                throw new ArgumentException(
                    $"Property type '{typeof(TProperty)} is not assignable to property of type '{@this.PropertyType.FullName}'.",
                    nameof(TProperty)
                );
            }

            if (!typeof(TInstance).IsAssignableTo(@this.DeclaringType))
            {
                throw new ArgumentException(
                    $"Property '{@this.Name}' is not declared on type '{typeof(TInstance).FullName}'.",
                    nameof(TInstance)
                );
            }

            var setMethod =
                @this.GetSetMethod(true)
                ?? throw new ArgumentException(
                    $"Property '{@this.Name}' does not have a setter nor init.",
                    nameof(@this)
                );

            var instanceParamExpr = Expression.Parameter(typeof(TInstance), "instance");
            var valueParamExpr = Expression.Parameter(typeof(TProperty), "value");
            var bodyExpr = Expression.Call(instanceParamExpr, setMethod, valueParamExpr);
            return Expression
                .Lambda<Action<TInstance, TProperty>>(bodyExpr, instanceParamExpr, valueParamExpr)
                .Compile();
        }
    }

    extension<TLimit, TActivatorData, TRegistrationStyle>(
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder
    )
    {
        /// <summary>
        /// Configures the registration to be identified by the specified symbol, allowing
        /// <typeparamref name="TService"/> to be resolved by that symbol as a key.
        /// </summary>
        /// <typeparam name="TService">The type of the service being registered.</typeparam>
        /// <param name="id">The symbol to identify the registration with.</param>
        /// <returns>The updated registration builder.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> IdentifiedBy<TService>(Symbol id)
            where TService : notnull
        {
            return builder.Keyed<TService>(id);
        }

        /// <summary>
        /// Configures the registration to be identified by the specified symbol, allowing it to be resolved by that
        /// <paramref name="serviceType"/> and symbol as a key.
        /// </summary>
        /// <param name="id">The symbol to identify the registration with.</param>
        /// <param name="serviceType">The type of the service being registered.</param>
        /// <returns>The updated registration builder.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> IdentifiedBy(
            Symbol id,
            Type serviceType
        )
        {
            return builder.Keyed(id, serviceType);
        }
    }

    extension(IComponentContext @this)
    {
        /// <summary>
        /// Resolves a <typeparamref name="T"/> service that is identified by the given symbol.
        /// </summary>
        /// <typeparam name="T">The type of the service to resolve.</typeparam>
        /// <param name="id">The symbol identifying the service.</param>
        /// <returns>The resolved service instance.</returns>
        public T ResolveIdentified<T>(Symbol id)
            where T : notnull
        {
            return @this.ResolveKeyed<T>(id);
        }

        /// <summary>
        /// Resolves a service of the specified <paramref name="serviceType"/> that is identified by the given symbol.
        /// </summary>
        /// <param name="id">The symbol identifying the service.</param>
        /// <param name="serviceType">The type of the service to resolve.</param>
        /// <returns>The resolved service instance.</returns>
        public object ResolveIdentified(Symbol id, Type serviceType)
        {
            return @this.ResolveKeyed(id, serviceType);
        }
    }

    extension(ParameterInfo @this)
    {
        /// <summary>
        /// Maps from a property-set-value parameter to the declaring property.
        /// </summary>
        /// <remarks>
        /// This method is copied from Autofac's internal helper by the same name.
        /// </remarks>
        /// <param name="prop">The property info on which the setter is specified.</param>
        /// <returns>True if the parameter is a property setter.</returns>
        public bool TryGetDeclaringProperty([NotNullWhen(returnValue: true)] out PropertyInfo? prop)
        {
            var mi = @this.Member as MethodInfo;
            if (
                mi is not null
                && mi.IsSpecialName
                && mi.Name.StartsWith("set_", StringComparison.Ordinal)
                && mi.DeclaringType is not null
            )
            {
                prop = mi.DeclaringType.GetDeclaredProperty(mi.Name[4..]);
                return true;
            }

            prop = null;
            return false;
        }
    }

    extension(ArgumentException)
    {
#if !NET6_0_OR_GREATER
        // polyfill the static ArgumentNullException methods

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the given argument is null.
        /// </summary>
        /// <remarks>
        /// This method is a polyfill for the static method included in .NET 6 and later.
        /// </remarks>
        /// <param name="argument">The argument to check for null.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">If the argument is null.</exception>
        public static void ThrowIfNull(object? argument, string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the given argument is null.
        /// </summary>
        /// <remarks>
        /// This method is a polyfill for the static method included in .NET 6 and later.
        /// </remarks>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="argument">The argument to check for null.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">If the argument is null.</exception>
        public static void ThrowIfNull<T>(T? argument, string? paramName = null)
            where T : class
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the given string argument is null or empty.
        /// </summary>
        /// <remarks>
        /// This method is a polyfill for the static method included in .NET 6 and later.
        /// </remarks>
        /// <param name="argument">The string argument to check for null or empty.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">If the argument is null or empty.</exception>
        public static void ThrowIfNullOrEmpty(string? argument, string? paramName = null)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the given string argument is null or consists only of white-space characters.
        /// </summary>
        /// <remarks>
        /// This method is a polyfill for the static method included in .NET 6 and later.
        /// </remarks>
        /// <param name="argument">The string argument to check for null or white-space.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">If the argument is null or consists only of white-space characters.</exception>
        public static void ThrowIfNullOrWhiteSpace(string? argument, string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentNullException(paramName);
            }
        }
#endif
    }

    extension<TDelegate>(Expression<TDelegate> @this)
        where TDelegate : Delegate
    {
        /// <summary>
        /// Tries to interpret this lambda expression as a member selector and returns the corresponding
        /// <see cref="MemberInfo"/> that was selected.
        /// </summary>
        /// <param name="member">The member info if the expression is a valid member selector; otherwise, null.</param>
        /// <returns>True if the expression is a valid member selector; otherwise, false.</returns>
        public bool TryGetSelectedMember([MaybeNullWhen(false)] out MemberInfo member)
        {
            if (@this.Body is not MemberExpression memberExpression)
            {
                member = null!;
                return false;
            }

            member = memberExpression.Member;
            return true;
        }

        /// <summary>
        /// Interprets this lambda expression as a member selector and returns the corresponding
        /// <see cref="MemberInfo"/> that was selected, throwing an exception if the expression is not a valid
        /// member selector.
        /// </summary>
        /// <returns>The member info for the selected member.</returns>
        /// <exception cref="ArgumentException">If the expression is not a valid property selector.</exception>
        public MemberInfo GetSelectedMember()
        {
            if (!@this.TryGetSelectedMember(out var member))
            {
                throw new ArgumentException($"The given expression '{@this}' is not a valid selector.", nameof(@this));
            }

            return member;
        }

        /// <summary>
        /// Tries to interpret this lambda expression as a property selector and returns the corresponding
        /// <see cref="PropertyInfo"/> that was selected.
        /// </summary>
        /// <param name="property">The property info if the expression is a valid property selector; otherwise, null.</param>
        /// <returns>True if the expression is a valid property selector; otherwise, false.</returns>
        public bool TryGetProperty([MaybeNullWhen(false)] out PropertyInfo property)
        {
            if (!@this.TryGetSelectedMember(out var member) || member is not PropertyInfo propertyInfo)
            {
                property = null!;
                return false;
            }

            property = propertyInfo;
            return true;
        }

        /// <summary>
        /// Interprets this lambda expression as a property selector and returns the corresponding
        /// <see cref="PropertyInfo"/> that was selected, throwing an exception if the expression is not a valid
        /// property selector.
        /// </summary>
        /// <returns>The property info for the selected property.</returns>
        /// <exception cref="ArgumentException">If the expression is not a valid property selector.</exception>
        public PropertyInfo GetSelectedProperty()
        {
            if (!@this.TryGetProperty(out var property))
            {
                throw new ArgumentException(
                    $"The given expression '{@this}' is not a valid property selector.",
                    nameof(@this)
                );
            }

            return property;
        }
    }

    extension(AppDomain @this)
    {
        /// <summary>
        /// Enumerates all the types in all the assemblies loaded in the application domain.
        /// </summary>
        /// <returns>An enumerable of all types in all loaded assemblies.</returns>
        public IEnumerable<Type> GetTypes()
        {
            return @this.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
        }
    }
}
