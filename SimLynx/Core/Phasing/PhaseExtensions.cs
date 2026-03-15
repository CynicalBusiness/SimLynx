
using System.Threading;
using System.Threading.Tasks;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Extension methods for phase-related infrastructure.
/// </summary>
public static class PhaseExtensions
{

    extension(IPhaseManager phaseManager)
    {

        /// <summary>
        /// Convenience method to start and run a phase manager in one call, returning a task that completes when the phase's
        /// run completes and finishes disposal.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <param name="phasePlan">An optional plan for which phases to run after this one, represented as an array of phase IDs. If the array is empty or null, no next phase will be run.</param>
        /// <returns>A task that represents the operation.</returns>
        public async Task StartAndRunAsync(
            Symbol[]? phasePlan = null,
            CancellationToken cancellationToken = default)
        {
            await using var phase = await phaseManager.StartAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            await phase.Value.StartAsync(phasePlan, cancellationToken);
        }

    }

}
