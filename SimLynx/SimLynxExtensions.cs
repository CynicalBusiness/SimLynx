using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
