using System;
using Autofac;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Hook for configuration of a phase.
/// </summary>
/// <param name="PhaseType">The type of the phase being configured.</param>
/// <param name="Builder">The container builder for registering dependencies.</param>
public record OnPhaseConfigure(Type PhaseType, ContainerBuilder Builder);

/// <summary>
/// Hook for configuration of a <typeparamref name="TPhase"/>.
/// </summary>
/// <typeparam name="TPhase">The type of the phase being configured.</typeparam>
/// <param name="Builder">The container builder for registering dependencies.</param>
public record OnPhaseConfigure<TPhase>(ContainerBuilder Builder) : OnPhaseConfigure(typeof(TPhase), Builder);
