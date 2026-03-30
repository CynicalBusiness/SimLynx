using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.OwnedInstances;

namespace SimLynx.Core.Phasing;

/// <summary>
/// Generalized manager for a phase of execution in SimLynx.
/// </summary>
public interface IPhaseBuilder
{
    /// <summary>
    /// Indicates whether or not this phase builder has completed its initialization.
    /// </summary>
    public bool IsInitialized { get; }

    /// <summary>
    /// Builds the phase, returning a task that completes when the phase is ready to run.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to abort the process.</param>
    public Task<IPhase> BuildAsync(CancellationToken cancellationToken);
}

/// <inheritdoc />
/// <typeparam name="TPhase">The type of phase this builder constructs.</typeparam>
public interface IPhaseBuilder<TPhase> : IPhaseBuilder
    where TPhase : IPhase
{
    /// <inheritdoc cref="IPhaseBuilder.BuildAsync(CancellationToken)"/>
    public new Task<TPhase> BuildAsync(CancellationToken cancellationToken);

    Task<IPhase> IPhaseBuilder.BuildAsync(CancellationToken cancellationToken) =>
        BuildAsync(cancellationToken).ContinueWith(task => (IPhase)task.Result, cancellationToken);
}
