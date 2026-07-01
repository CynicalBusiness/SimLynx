using System;
using System.Collections.Generic;
using System.Linq;
using SimLynx.Core.Prototyping.Blueprints;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// Prototype config service which can compile a particular type of config into a delegate which can be used to apply
/// the config to a subject.
/// </summary>
public interface IPrototypeConfigCompiler : IPrototypeConfigService
{
    /// <summary>
    /// Compiles the given configs into a delegate which can be used to apply the configs to a subject.
    /// </summary>
    /// <typeparam name="TSubject">The type of the subject to which the configs will be applied.</typeparam>
    /// <param name="configs">The configs to compile.</param>
    /// <param name="blueprintBuilder">The blueprint builder to which the configs will be applied.</param>
    public void Compile<TSubject>(IEnumerable<IPrototypeConfig> configs, BlueprintBuilder<TSubject> blueprintBuilder)
        where TSubject : class, IPrototypeSubject;
}

/// <summary>
/// Prototype config service which can compile a particular type of config into a delegate which can be used to apply
/// the config to a subject.
/// </summary>
/// <typeparam name="TConfig">The type of configuration this compiler can handle.</typeparam>
public interface IPrototypeConfigCompiler<in TConfig> : IPrototypeConfigCompiler
    where TConfig : IPrototypeConfig
{
    /// <inheritdoc cref="IPrototypeConfigCompiler.Compile{TSubject}(IEnumerable{IPrototypeConfig}, BlueprintBuilder{TSubject})"/>
    public void Compile<TSubject>(IEnumerable<TConfig> configs, BlueprintBuilder<TSubject> blueprintBuilder)
        where TSubject : class, IPrototypeSubject;

    void IPrototypeConfigCompiler.Compile<TSubject>(
        IEnumerable<IPrototypeConfig> configs,
        BlueprintBuilder<TSubject> blueprintBuilder
    )
    {
        Compile(configs.Cast<TConfig>(), blueprintBuilder);
    }
}
