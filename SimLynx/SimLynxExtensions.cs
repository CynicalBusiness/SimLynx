using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    /// <param name="source">The source sequence.</param>
    extension<T>(IEnumerable<T> source)
    {
        /// <summary>
        /// Performs the specified action on each element of the source sequence and yields the element.
        /// </summary>
        /// <param name="action">The action to perform on each element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that yields the elements of the source sequence after performing the action on each element.</returns>
        public IEnumerable<T> Tap(Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
                yield return item;
            }
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
                if (next.IsGenericType && next.GetGenericTypeDefinition() == generic)
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
}
