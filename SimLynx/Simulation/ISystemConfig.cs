using System;

namespace SimLynx.Simulation;

/// <summary>
/// Information for configuring a system in SimLynx's simulation engine.
/// </summary>
public interface ISystemConfig
{
    /// <summary>
    /// The concrete type of system being configured.
    /// </summary>
    public Type SystemType { get; }
}

/// <inheritdoc />
/// <typeparam name="TSystem">The type of system being configured.</typeparam>
public interface ISystemConfig<TSystem> : ISystemConfig
    where TSystem : ISystem
{
    Type ISystemConfig.SystemType => typeof(TSystem);
}
