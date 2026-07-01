namespace SimLynx.Core.Prototyping;

/// <summary>
/// Defines a resolver for prototype configs, which can both provide and compile them.
/// </summary>
public interface IPrototypeConfigResolver : IPrototypeConfigProvider, IPrototypeConfigCompiler { }

/// <summary>
/// Defines a resolver for prototype configs of type <typeparamref name="TConfig"/>, which can both provide and
/// compile them.
/// </summary>
/// <typeparam name="TConfig">The type of prototype config this resolver handles.</typeparam>
public interface IPrototypeConfigResolver<TConfig>
    : IPrototypeConfigResolver,
        IPrototypeConfigProvider<TConfig>,
        IPrototypeConfigCompiler<TConfig>
    where TConfig : IPrototypeConfig { }
