using System.Threading;
using System.Threading.Tasks;
using Autofac;
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
        /// <typeparam name="TPhase"></typeparam>
        /// <typeparam name="TPhaseManager"></typeparam>
        /// <param name="phaseId"></param>
        /// <returns></returns>
        public IModuleRegistrar RegisterPhase<TPhase, TPhaseManager>(string phaseId)
            where TPhase : class, IPhase
            where TPhaseManager : class, IPhaseBuilder<TPhase>
        {
            return builder.RegisterModule(new PhaseModule<TPhase, TPhaseManager>(phaseId));
        }
    }

    extension(ILifetimeScope scope)
    {
        /// <inheritdoc cref="IPhaseManager.StartAsync"/>
        public Task BeginPhase(string phaseId, CancellationToken cancellationToken = default)
        {
            return scope.Resolve<IPhaseManager>().StartAsync(phaseId, cancellationToken);
        }
    }
}
