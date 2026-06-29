using System;
using System.Collections.Generic;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// Prototype config service which can compile a particular type of config into a delegate which can be used to apply
/// the config to a subject.
/// </summary>
/// <typeparam name="TConfig">The type of configuration this compiler can handle.</typeparam>
public interface IPrototypeConfigCompiler<in TConfig> : IPrototypeConfigService
    where TConfig : IPrototypeConfig
{
    /// <summary>
    /// Compiles the given configs into a delegate which can be used to apply the configs to a subject.
    /// </summary>
    /// <typeparam name="TSubject">The type of the subject to which the configs will be applied.</typeparam>
    /// <param name="configs">The configs to compile.</param>
    /// <returns>A delegate that applies the compiled configs to a subject.</returns>
    public Action<TSubject> Compile<TSubject>(IEnumerable<TConfig> configs);
}
