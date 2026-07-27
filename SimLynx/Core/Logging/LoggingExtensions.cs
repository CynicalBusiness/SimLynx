using System;
using Microsoft.Extensions.Logging;

namespace SimLynx.Core.Logging;

/// <summary>
/// Extensions for logging in SimLynx.
/// </summary>
public static class LoggingExtensions
{
    extension(EventId)
    {
        /// <summary>
        /// Creates an <see cref="EventId"/> for the given defining type and event name.
        /// </summary>
        /// <param name="definingType">The type that defines the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <returns>An <see cref="EventId"/> representing the event.</returns>
        public static EventId For(Type definingType, string eventName)
        {
            var name = $"{definingType}#{eventName}";
            return new EventId(Symbol.For(name).Value, name); // `Symbol.For` keeps event IDs stable across runs.
        }

        /// <inheritdoc cref="For(Type, string)"/>
        /// <typeparam name="T">The type that defines the event.</typeparam>
        public static EventId For<T>(string eventName)
        {
            return For(typeof(T), eventName);
        }
    }
}
