using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using SimLynx.Core;
using SimLynx.Core.Hooks;
using SimLynx.Core.Phasing;

namespace SimLynx.Testing;

/// <summary>
/// A testing fixture designed to test content and functionality which belongs to a specific phase of SimLynx.
/// <br/>
/// This fixture will, upon being constructed, create a new SimLynx instance/container and runs the app until
/// the specified phase is reached. This fixture will hold the app at that phase until disposed.
/// </summary>
/// <remarks>
/// To use the fixture, create a subclass of it and implement the necessary members.
/// </remarks>
/// <typeparam name="TApp">The type of the SimLynx application being tested.</typeparam>
/// <typeparam name="TPhase">The type of the phase being tested.</typeparam>
public abstract class PhaseFixture<TApp, TPhase> : SimLynxFixture<TApp>
    where TApp : SimLynxApp
    where TPhase : Phase
{
    private readonly CancellationTokenSource cts = new();

    private readonly Listener listener;

    /// <summary>
    /// Initializes a new fixture with an optional <paramref name="parentScope"/>.
    /// </summary>
    /// <param name="options">Options for configuring the behavior of the fixture.</param>
    /// <param name="parentScope">The parent lifetime scope for the fixture's container.</param>
    public PhaseFixture(Options options, ILifetimeScope? parentScope = null)
        : base(parentScope)
    {
        var plan = App.CreatePhasePlan().Until<TPhase>();
        listener = Container.Resolve<Listener>(new TypedParameter(typeof(Options), options));

        // time for some sneaky shenanigans:
        // we want to run the app until the phase is reached and hold it there,
        // but we need to ensure the constructor unblocks when that point is reached,
        // so we need to run the app in a background task and wait for the phase to be reached.
        // and we also need to extract the scope for the phase so that we can use it in the test, which we *do* have to block for
        // note: Autofac is not thread-safe, so need to make sure no two things happen at once

        // spawn the phase run off on another thread so we can block the constructor until the phase is reached
        var runTask = Task.Run(async () => await Container.BeginPhase(plan, cts.Token), cts.Token)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // forward exceptions!
                    listener.TryFail(t.Exception);
                }
                else if (t.IsCompletedSuccessfully)
                {
                    // the task completing means the phase(s) completed potentially unexpectedly, so notify the listener
                    // if the listener has already yielded its result, this will do nothing
                    // if not, this completion is unexpected and the constructor should throw
                    listener.TryFail(
                        new InvalidOperationException(
                            "Phase run completed before the fixture's phase hooks were run. Does the phase exist in the plan?"
                        )
                    );
                }
            });

        // listener will hook into the phase and resolve its scope task when it has stopped and is waiting
        PhaseScope = listener.PhaseScopeTask.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initializes a new fixture with default options and an optional <paramref name="parentScope"/>.
    /// </summary>
    /// <param name="parentScope">The parent lifetime scope for the fixture's container.</param>
    public PhaseFixture(ILifetimeScope? parentScope = null)
        : this(Options.Default, parentScope) { }

    /// <summary>
    /// Scope for the phase being tested.
    /// </summary>
    public ILifetimeScope PhaseScope { get; }

    /// <inheritdoc/>
    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        base.ConfigureContainer(builder);

        builder.RegisterType<Listener>().AsSelf().AsImplementedInterfaces().SingleInstance();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        cts.Cancel();
        listener.Dispose();
        base.Dispose();
    }

    /// <summary>
    /// Options for configuring the behavior of the <see cref="PhaseFixture{TApp, TPhase}"/>.
    /// </summary>
    public record Options
    {
        /// <summary>
        /// Default options.
        /// </summary>
        public static readonly Options Default = new();

        /// <summary>
        /// Indicates whether the fixture should run the phase. If set, the fixture waits for the entire phase to run
        /// before holding; if not, the phase is only initialized and the fixture holds before it runs.
        /// </summary>
        public bool ShouldRunPhase { get; init; } = true;
    }

    private class Listener : IDisposable
    {
        private readonly TaskCompletionSource<ILifetimeScope> phaseScopeSource = new();
        private readonly TaskCompletionSource<bool> phaseRunSource = new();

        private ILifetimeScope? phaseScope;

        public Listener(Options options, Hook<OnPhaseConfigure<TPhase>> onInit, Hook<OnPhaseRun<TPhase>> onRun)
        {
            // hook into init first so we have the scope for the phase before it runs.
            onInit.Subscribe(
                new HookHandler<OnPhaseConfigure<TPhase>>(
                    (payload, ctx) =>
                    {
                        payload.Builder.RegisterBuildCallback(scope => phaseScope = scope);
                        return Task.CompletedTask;
                    }
                )
                {
                    Priority = Priorities.Max,
                }
            );

            // hook into run absolutely last so the phase runs completely, but hold it there
            onRun.Subscribe(
                new HookHandler<OnPhaseRun<TPhase>>(
                    async (payload, ctx) =>
                    {
                        if (phaseScope is null)
                        {
                            throw new InvalidOperationException("Phase scope is null. Did the phase run before init?");
                        }

                        phaseScopeSource.TrySetResult(phaseScope);

                        // wait for the fixture to be disposed before allowing the phase to continue
                        await phaseRunSource.Task;
                    }
                )
                {
                    // if we are running the phase, we want to be last so it runs, otherwise be first so it doesn't get a chance to
                    Priority = options.ShouldRunPhase ? Priorities.Min : Priorities.Max,
                }
            );
        }

        public Task<ILifetimeScope> PhaseScopeTask => phaseScopeSource.Task;
        public Task PhaseRunTask => phaseRunSource.Task;

        public bool TryFail(Exception ex)
        {
            return phaseScopeSource.TrySetException(ex) || phaseRunSource.TrySetException(ex);
        }

        public void Dispose()
        {
            phaseRunSource.TrySetResult(true);
            phaseScopeSource.TrySetCanceled();
        }
    }
}
