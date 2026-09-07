using System;
using System.Diagnostics.CodeAnalysis;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// General prototype property which configures a value.
/// </summary>
public interface IPrototypeValueConfig : IPrototypeConfig
{
    /// <summary>
    /// The type of the value for this configuration.
    /// </summary>
    public Type ValueType { get; }

    /// <summary>
    /// Whether this configuration has a value to apply to the target property.
    /// </summary>
    /// <remarks>
    /// A configuration for a required property which has no value may be an error if the type is unable to be constructed.
    /// </remarks>
    public bool HasValue { get; }

    /// <summary>
    /// Whether this configuration has any configurations to apply to the value.
    /// </summary>
    public bool HasConfigurations { get; }

    /// <summary>
    /// Attempts to create a provider function for the value from this configuration.
    /// </summary>
    /// <param name="provider">The created provider function if successful.</param>
    /// <returns><c>true</c> if a provider function was successfully created; otherwise, <c>false</c>.</returns>
    public bool TryCreateProvider([MaybeNullWhen(false)] out Func<object?>? provider);

    /// <summary>
    /// Attempts to create a value from this configuration.
    /// </summary>
    /// <param name="value">The created value if successful.</param>
    /// <returns><c>true</c> if a value was successfully created; otherwise, <c>false</c>.</returns>
    public bool TryCreateValue([MaybeNullWhen(false)] out object? value);

    /// <summary>
    /// Configures this config with a new <paramref name="valueFactory"/>, replacing the existing
    /// one, if any.
    /// </summary>
    /// <param name="valueFactory">The function to provide the constant value for the property.</param>
    public void Configure(Func<object?> valueFactory);

    /// <summary>
    /// Applies a configuration function to the value created by this configuration, if any.
    /// </summary>
    /// <param name="configurationFunc">The function to apply to the value.</param>
    public void Configure(Func<object?, object?> configurationFunc);

    /// <summary>
    /// Applies a configuration action to the value created by this configuration, if any.
    /// </summary>
    /// <param name="configuration">The action to apply to the value.</param>
    public void Configure(Action<object?> configuration);
}

/// <summary>
/// A prototype config which configures a <typeparamref name="TValue"/> value.
/// </summary>
/// <typeparam name="TValue">The type of value this configures.</typeparam>
public interface IPrototypeValueConfigOf<TValue> : IPrototypeValueConfig
{
    /// <summary>
    /// The current factory function to create a value from this configuration, if any.
    /// </summary>
    public Func<TValue>? ValueFactory { get; set; }

    /// <inheritdoc cref="IPrototypeValueConfig.TryCreateProvider"/>
    public bool TryCreateProvider([MaybeNullWhen(false)] out Func<TValue>? provider);

    bool IPrototypeValueConfig.TryCreateProvider([MaybeNullWhen(false)] out Func<object?>? provider)
    {
        var result = TryCreateProvider(out var typedProvider);
        provider = typedProvider is not null ? () => typedProvider() : null;
        return result;
    }

    /// <inheritdoc cref="IPrototypeValueConfig.TryCreateValue"/>
    public bool TryCreateValue([MaybeNullWhen(false)] out TValue value);

    bool IPrototypeValueConfig.TryCreateValue(out object? value)
    {
        var result = TryCreateValue(out var typedValue);
        value = typedValue;
        return result;
    }

    /// <inheritdoc cref="IPrototypeValueConfig.Configure(Func{object?})"/>
    public void Configure(Func<TValue> valueFunc);

    void IPrototypeValueConfig.Configure(Func<object?> valueFactory)
    {
        Configure(() => (TValue)valueFactory()!);
    }

    /// <inheritdoc cref="IPrototypeValueConfig.Configure(Func{object?, object?})"/>
    public void Configure(Func<TValue, TValue> configurationFunc);

    void IPrototypeValueConfig.Configure(Func<object?, object?> configurationFunc)
    {
        Configure(value => (TValue)configurationFunc(value)!);
    }

    /// <inheritdoc cref="IPrototypeValueConfig.Configure(Action{object?})"/>
    public void Configure(Action<TValue> configuration);

    void IPrototypeValueConfig.Configure(Action<object?> configuration)
    {
        Configure(value => configuration(value));
    }
}

/// <summary>
/// A prototype config which configures a <typeparamref name="TValue"/> value for a
/// <typeparamref name="TSubject"/> subject type.
/// </summary>
/// <typeparam name="TSubject">The type of the subject this configures.</typeparam>
/// <typeparam name="TValue">The type of value this configures.</typeparam>
public interface IPrototypeValueConfig<TSubject, TValue> : IPrototypeConfig<TSubject>, IPrototypeValueConfigOf<TValue>
    where TSubject : class, IPrototypeSubject { }
