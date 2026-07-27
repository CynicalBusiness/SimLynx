using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Autofac.Core.Registration;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Extension methods for phase-related infrastructure.
/// </summary>
public static class PhaseExtensions
{
    extension(ContainerBuilder builder)
    {
        /// <summary>
        /// Registers a phase and its manager in the DI container, with the given phase ID as the key.
        /// </summary>
        /// <typeparam name="TPhase">The type of phase to register</typeparam>
        /// <returns></returns>
        public IModuleRegistrar RegisterPhase<TPhase>()
            where TPhase : class, IPhase
        {
            return builder.RegisterModule(new PhaseModule<TPhase>());
        }
    }

    extension<TLimit, TData, TStyle>(IRegistrationBuilder<TLimit, TData, TStyle> @this)
    {
        /// <summary>
        /// Configures a service to be registered such that it will have one instance per <paramref name="phaseType"/>.
        /// </summary>
        /// <remarks>
        /// Follows the behavior of
        /// <see cref="IRegistrationBuilder{TLimit, TData, TStyle}.InstancePerMatchingLifetimeScope"/> with the
        /// appropriate tag for the phase.
        /// </remarks>
        /// <param name="phaseType">The phase type whose lifetime scope should own the instance.</param>
        /// <returns>The registration builder for chaining.</returns>
        public IRegistrationBuilder<TLimit, TData, TStyle> InstancePerPhase(Type phaseType)
        {
            var reg = @this.InstancePerLifetimeScope();
            reg.RegistrationData.Lifetime = new PhaseTypeScopeLifetime(phaseType);
            return reg;
        }
    }

    extension(ILifetimeScope scope)
    {
        /// <inheritdoc cref="PhaseRunner.StartAsync"/>
        public Task BeginPhase(PhasePlan phasePlan, CancellationToken cancellationToken = default)
        {
            return scope.Resolve<PhaseRunner>().StartAsync(phasePlan, cancellationToken);
        }
    }
}
