
using System;
using System.Diagnostics.CodeAnalysis;

namespace SimLynx;

/// <summary>
/// Exception indicating a type was not of the expected type.
/// </summary>
public class TypeException : Exception
{
    /// <summary>
    /// Creates a new type exception indicating that the specified <paramref name="type"/> is not
    /// assignable to the <paramref name="expectedType"/>.
    /// </summary>
    /// <param name="type">The type that caused the exception.</param>
    /// <param name="expectedType">The expected type that was not met by the type.</param>
    /// <returns>A new <see cref="TypeException"/> instance.</returns>
    public static TypeException NotAssignable(Type type, Type expectedType)
        => new(type, expectedType);

    /// <summary>
    /// Creates a new type exception indicating that the specified <paramref name="type"/> is not
    /// assignable to the <typeparamref name="TExpected"/>.
    /// </summary>
    /// <typeparam name="TExpected">The expected type that was not met by the type.</typeparam>
    /// <param name="type">The type that caused the exception.</param>
    /// <returns>A new <see cref="TypeException"/> instance.</returns>
    public static TypeException NotAssignable<TExpected>(Type type)
        => NotAssignable(type, typeof(TExpected));


    /// <summary>
    /// Throws a <see cref="TypeException"/> if the specified <paramref name="type"/> is not assignable to the
    /// <paramref name="expectedType"/>.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="expectedType">The expected type that the <paramref name="type"/> should be assignable to.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <exception cref="TypeException"></exception>
    public static void ThrowIfNotAssignable(Type type, Type expectedType, string? paramName = null)
    {
        if (!expectedType.IsAssignableFrom(type))
        {
            throw string.IsNullOrWhiteSpace(paramName)
                ? NotAssignable(type, expectedType)
                : new ArgumentException(
                    $"Provided type is not valid.",
                    paramName,
                    NotAssignable(type, expectedType));
        }
    }

    /// <summary>
    /// Throws a <see cref="TypeException"/> if the specified <paramref name="type"/> is not assignable to
    /// <typeparamref name="TExpected"/>.
    /// </summary>
    /// <typeparam name="TExpected">The expected type that the <paramref name="type"/> should be assignable to.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <exception cref="TypeException"></exception>
    public static void ThrowIfNotAssignable<TExpected>(Type type, string? paramName = null)
    {
        ThrowIfNotAssignable(type, typeof(TExpected), paramName);
    }

    /// <summary>
    /// Throws a <see cref="TypeException"/> if the specified <paramref name="condition"/> is not met for the
    /// given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="expectation">The expectation that was not met by the type.</param>
    /// <exception cref="TypeException"></exception>
    public static void ThrowIfNot(Type type, [DoesNotReturnIf(false)] bool condition, string expectation)
    {
        if (!condition)
        {
            throw new TypeException(type, expectation);
        }
    }

    /// <summary>
    /// Throws a <see cref="TypeException"/> if the specified <paramref name="condition"/> is met for the
    /// given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="expectation">The expectation that was not met by the type.</param>
    /// <exception cref="TypeException"></exception>
    public static void ThrowIf(Type type, [DoesNotReturnIf(true)] bool condition, string expectation)
    {
        if (condition)
        {
            throw new TypeException(type, expectation);
        }
    }

    /// <summary>
    /// Throws a <see cref="TypeException"/> if the specified <paramref name="obj"/> is <see langword="null"/>.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="obj">The object to check for null.</param>
    /// <param name="expectation">The expectation that was not met by the type.</param>
    /// <exception cref="TypeException"></exception>
    public static void ThrowIfNull(Type type, [NotNull] object? obj, string expectation)
    {
        if (obj is null)
        {
            throw new TypeException(type, expectation);
        }
    }

    /// <summary>
    /// Creates a new type exception with the specified <paramref name="message"/>.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public TypeException(string message)
        : base(message)
    { }

    /// <summary>
    /// Creates a new type exception with the specified <paramref name="message"/> and <paramref name="inner"/> exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public TypeException(string message, Exception inner)
        : base(message, inner)
    { }

    /// <summary>
    /// Creates a new type exception for the specified <paramref name="type"/> and <paramref name="expectation"/>.
    /// </summary>
    /// <param name="type">The type that caused the exception.</param>
    /// <param name="expectation">The expectation that was not met by the type.</param>
    public TypeException(Type type, string expectation)
        : base($"Expected that type '{type.FullName}' {expectation}")
    { }

    /// <summary>
    /// Creates a new type exception for the specified <paramref name="type"/> and <paramref name="expectedType"/>.
    /// </summary>
    /// <param name="type">The type that caused the exception.</param>
    /// <param name="expectedType">The expected type that was not met by the type.</param>
    public TypeException(Type type, Type expectedType)
        : this(type, $"is assignable to type '{expectedType.FullName}'")
    { }
}
