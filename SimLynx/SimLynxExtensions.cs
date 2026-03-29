using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Autofac.Builder;

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

    extension(Type thisType)
    {
        /// <summary>
        /// Determines if the specified type is a subclass of a raw generic type.
        /// </summary>
        /// <param name="generic">The generic type to check against.</param>
        /// <param name="bailAtType">The type at which to stop checking the inheritance chain, or <c>null</c> to check all the way up the hierarchy.</param>
        /// <param name="found">The found closed generic type, if any.</param>
        /// <returns><c>true</c> if the type is a subclass of the specified raw generic type; otherwise, <c>false</c>.</returns>
        public bool IsSubclassOfRawGeneric(Type generic, Type? bailAtType, [MaybeNullWhen(false)] out Type found)
        {
            Type? next = thisType;
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

        /// <inheritdoc cref="IsSubclassOfRawGeneric(Type, Type?, out Type)"/>
        public bool IsSubclassOfRawGeneric(Type generic, [MaybeNullWhen(false)] out Type found)
        {
            return IsSubclassOfRawGeneric(thisType, generic, null, out found);
        }

        /// <inheritdoc cref="IsSubclassOfRawGeneric(Type, Type?, out Type)"/>
        public bool IsSubclassOfRawGeneric(Type generic)
        {
            return IsSubclassOfRawGeneric(thisType, generic, null, out _);
        }

        /// <inheritdoc cref="IsSubclassOfRawGeneric(Type, Type?, out Type)"/>
        public bool IsSubclassOfRawGeneric(Type generic, Type? bailAtType)
        {
            return IsSubclassOfRawGeneric(thisType, generic, bailAtType, out _);
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
            return targetType.IsAssignableFrom(thisType);
        }
#endif
    }

    extension(PropertyInfo propertyInfo)
    {
        /// <summary>
        /// Indicates whether or not this property is required on its type.
        /// </summary>
        /// <remarks>
        /// A property is considered required if it uses the <see langword="required"/> modifier or marked with
        /// <see cref="RequiredAttribute"/>.
        /// </remarks>
        public bool IsRequired =>
            propertyInfo.IsDefined(typeof(RequiredAttribute))
#if NET5_0_OR_GREATER
            || propertyInfo.IsDefined(typeof(System.Runtime.CompilerServices.RequiredMemberAttribute), inherit: false);
#else
            // in case a down-stream consumer uses their own shim, or the compiler includes it itself, look by name
            || propertyInfo.CustomAttributes.Any(attribute =>
                attribute.AttributeType.FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute"
            );
#endif
    }

    extension<TLimit, TActivatorData, TRegistrationStyle>(
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder
    )
    {
        /// <summary>
        /// Configures the registration to be identified by the specified symbol, allowing it to be resolved by that
        /// symbol as a key.
        /// </summary>
        /// <param name="id">The symbol to identify the registration with.</param>
        /// <returns>The updated registration builder.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> IdentifiedBy(Symbol id)
        {
            return builder.Keyed<Symbol>(id);
        }
    }
}
