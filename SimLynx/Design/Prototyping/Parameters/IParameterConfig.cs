using Autofac.Core;

namespace SimLynx.Design.Prototyping.Parameters;

/// <summary>
/// A prototype config which configures an Autofac injection parameter.
/// </summary>
public interface IParameterConfig : IPrototypeValueConfigOf<Parameter> { }

/// <inheritdoc cref="IParameterConfig"/>
/// <typeparam name="TSubject">The prototype subject type</typeparam>
public interface IParameterConfig<TSubject> : IParameterConfig, IPrototypeValueConfig<TSubject, Parameter>
    where TSubject : class, IPrototypeSubject { }
