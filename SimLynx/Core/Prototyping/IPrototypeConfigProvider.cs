namespace SimLynx.Core.Prototyping;

/// <summary>
/// A provider for a specific "slot" on a prototype which configs can be registered to.
/// </summary>
public interface IPrototypeConfigProvider : IPrototypeConfigService
{
    /// <summary>
    /// Attempts to create a configuration for the given prototype with the given name, if possible.
    /// </summary>
    /// <remarks>
    /// This method should return <c>null</c> to indicate that the provider does not support creating a configuration
    /// with the provided <paramref name="name"/> on the provided <paramref name="prototype"/>, which will trigger the
    /// caller to search for another provider which can. If the config creation is <em>supported</em> but is somehow
    /// invalid (eg. name is missing/invalid, etc.), throw an exception instead.
    /// </remarks>
    /// <typeparam name="TSubject">The subject type of the prototype.</typeparam>
    /// <param name="prototype">The prototype for which to create the configuration.</param>
    /// <param name="name">The name of the configuration.</param>
    /// <returns>The created configuration, if successful; otherwise, <c>null</c>.</returns>
    public IPrototypeConfig? TryCreate<TSubject>(IPrototype<TSubject> prototype, string name)
        where TSubject : class, IPrototypeSubject;
}

/// <summary>
/// Provider of <typeparamref name="TConfig"/> prototype configs for a specific "slot" on a prototype.
/// </summary>
/// <typeparam name="TConfig">The type of configuration this provider creates.</typeparam>
public interface IPrototypeConfigProvider<out TConfig> : IPrototypeConfigProvider
    where TConfig : IPrototypeConfig
{
    /// <inheritdoc cref="IPrototypeConfigProvider.TryCreate"/>
    public new TConfig? TryCreate<TSubject>(IPrototype<TSubject> prototype, string name)
        where TSubject : class, IPrototypeSubject;

    IPrototypeConfig? IPrototypeConfigProvider.TryCreate<TSubject>(IPrototype<TSubject> prototype, string name)
    {
        return TryCreate(prototype, name);
    }
}
