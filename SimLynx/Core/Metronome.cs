using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SimLynx.Core.Hooks;

namespace SimLynx.Core;

/// <summary>
/// A specialized timer that can be used to trigger events, even async ones, at a consistent rate while still remaining
/// adjustable and pausable. Intended to be used as a central timing mechanism for looping actions, such as the
/// main simulation loop.
/// </summary>
/// <remarks>
/// This class will begin operating once <see cref="Start()"/> is called, and the returned task will
/// only complete once the metronome has stopped.
/// </remarks>
public class Metronome : IDisposable, IHookable<Metronome.OnTick>
{
    /// <summary>
    /// Prefix for the thread name of the metronome's internal timer thread.
    /// </summary>
    public const string THREAD_NAME_PREFIX = "Metronome";

    /// <summary>
    /// Delegate type for the <see cref="Ticked"/> event.
    /// </summary>
    /// <param name="tick">The tick information.</param>
    /// <param name="remainingTime">The remaining wait time until the next tick, if any.</param>
    public delegate void TickedEventHandler(OnTick tick, TimeSpan remainingTime);

    private readonly ReaderWriterLockSlim timerLock = new();
    private readonly Stopwatch timer = new();
    private readonly Hook<OnTick> onTick;
    private readonly Thread timerThread;
    private readonly TaskCompletionSource<bool> completion = new();
    private readonly CancellationTokenSource cancellation = new();

    /// <summary>
    /// Event that is triggered after each tick of the metronome.
    /// </summary>
    /// <remarks>
    /// <strong>DO NOT</strong> use this event for performing actual tick operations. Instead, use the
    /// <see cref="OnTick"/> hook.
    /// <br/>
    /// This event is invoked by the metronome's timing thread. Consumers should be mindful of thread safety of any
    /// operations performed in response to this event, and should avoid blocking operations to prevent delaying
    /// subsequent ticks.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public event TickedEventHandler? Ticked;

    /// <summary>
    /// Initializes a new instance of <see cref="Metronome"/> with the given <paramref name="deliveryStrategy"/> for
    /// the internal tick hook.
    /// </summary>
    /// <param name="initialTickRate">The initial rate at which the metronome should tick.</param>
    /// <param name="deliveryStrategy">The delivery strategy to use for tick events.</param>
    public Metronome(TimeSpan initialTickRate, IHookDeliveryStrategy deliveryStrategy)
    {
        onTick = new(deliveryStrategy);
        timerThread = new Thread(CreateThread()) { IsBackground = true };

        TickRate = initialTickRate;
    }

    /// <summary>
    /// The name of this metronome, used for identification and debugging purposes. This will be reflected in the name
    /// of the metronome's internal timer thread.
    /// </summary>
    public string? Name
    {
        get => field;
        set
        {
            field = value;
            timerThread.Name = string.IsNullOrWhiteSpace(value) ? THREAD_NAME_PREFIX : $"{THREAD_NAME_PREFIX}: {value}";
        }
    } = null;

    /// <summary>
    /// The target rate at which the metronome will tick.
    /// </summary>
    /// <remarks>
    /// The actual rate may vary slightly and its exact value will be provided as <see cref="OnTick.Delta"/>.
    /// </remarks>
    public TimeSpan TickRate
    {
        get => field;
        set
        {
            if (value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Tick rate must be a positive value.");
            }

            field = value;
            timerThread.Interrupt();
        }
    }

    /// <summary>
    /// A task that will complete once this metronome is stopped.
    /// </summary>
    public Task Task => completion.Task;

    /// <summary>
    /// Whether this metronome is currently paused. When <c>true</c>, the metronome will temporarily stop
    /// invoking tick events until resumed by setting to <c>false</c>.
    /// </summary>
    /// <remarks>
    /// If the metronome is not running, this property has no effect and will return <c>true</c>.
    /// </remarks>
    public bool Paused
    {
        get => !timer.IsRunning;
        set
        {
            lock (timer)
            {
                if (value == Paused)
                {
                    return;
                }

                if (value)
                {
                    timer.Stop();
                    timerThread.Interrupt();
                }
                else
                {
                    timer.Start();
                    if (timerThread.ThreadState.HasFlag(System.Threading.ThreadState.Unstarted))
                    {
                        timerThread.Start();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Indicates whether this metronome is currently active, meaning it has been started and has not yet completed or
    /// faulted.
    /// </summary>
    public bool IsActive => timerThread.IsAlive;

    /// <summary>
    /// The current state of this metronome.
    /// </summary>
    public OperationalState State
    {
        get
        {
            if (Task.IsCompleted)
            {
                return Task.IsCompletedSuccessfully ? OperationalState.Finished : OperationalState.Failed;
            }

            if (timerThread.ThreadState == System.Threading.ThreadState.Unstarted)
            {
                return OperationalState.Inactive;
            }
            else if (!IsActive)
            {
                return OperationalState.Invalid;
            }

            return Paused ? OperationalState.Suspended : OperationalState.Active;
        }
    }

    /// <summary>
    /// Pauses this metronome, temporarily pausing invoking tick events until <see cref="Resume()"/> is called.
    /// </summary>
    public void Pause()
    {
        Paused = true;
    }

    /// <summary>
    /// Resumes this metronome, allowing tick events to be invoked again.
    /// </summary>
    public void Resume()
    {
        Paused = false;
    }

    /// <summary>
    /// Starts this metronome, if not already, and returns a task that will complete once the metronome is stopped.
    /// </summary>
    /// <remarks>
    /// If the metronome is already running, nothing is done and the same returned task will be returned.
    /// </remarks>
    /// <returns>A lifetime task</returns>
    public Task Start()
    {
        Resume();
        return Task;
    }

    /// <summary>
    /// Stops this metronome, cancelling any active event invocations and stopping the internal timer.
    /// </summary>
    public void Stop()
    {
        if (cancellation.IsCancellationRequested)
        {
            return;
        }

        GC.SuppressFinalize(this);

        // cancelling the token will signal the thread to stop, and it will take care of cleanup.
        cancellation.Cancel();
        timerThread.Interrupt();
    }

    /// <summary>
    /// Alias for <see cref="Stop()"/>
    /// </summary>
    public void Dispose()
    {
        Stop();
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IHookHandler<OnTick> handler)
    {
        return onTick.Subscribe(handler);
    }

    private ThreadStart CreateThread()
    {
        return () =>
        {
            var executionTimer = new Stopwatch();

            while (!cancellation.IsCancellationRequested)
            {
                timerLock.EnterUpgradeableReadLock();

                try
                {
                    // if the timer isn't running, we're paused, so just go to sleep.
                    if (!timer.IsRunning)
                    {
                        timerLock.ExitUpgradeableReadLock();
                        Thread.Sleep(Timeout.Infinite);
                        continue;
                    }

                    // grab a delta off our timer stopwatch
                    // and restart it to start counting for the next tick
                    timerLock.EnterWriteLock();
                    var delta = timer.Elapsed;
                    timer.Restart();
                    timerLock.ExitWriteLock();
                    timerLock.ExitUpgradeableReadLock();

                    // invoke the tick hook, keeping track of how long it takes
                    var tick = new OnTick(Metronome: this, Delta: delta);
                    executionTimer.Restart();
                    onTick.Invoke(tick, cancellation.Token).GetAwaiter().GetResult();
                    executionTimer.Stop();

                    // now sleep for the remaining time until the next tick, if any, then loop again
                    var remainingTime = TickRate - executionTimer.Elapsed;
                    Ticked?.Invoke(tick, remainingTime);
                    if (remainingTime > TimeSpan.Zero)
                    {
                        Thread.Sleep(remainingTime);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    // interrupt just means "go try the loop again"
                    continue;
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                    break;
                }
                finally
                {
                    if (timerLock.IsWriteLockHeld)
                    {
                        timerLock.ExitWriteLock();
                    }

                    if (timerLock.IsUpgradeableReadLockHeld)
                    {
                        timerLock.ExitUpgradeableReadLock();
                    }
                }
            }

            // once the loop breaks out, clean up
            completion.TrySetResult(true);
            timerLock.Dispose();
            timer.Stop();
        };
    }

    /// <summary>
    /// Finalizer to ensure the internal timer thread is stopped and resources are cleaned up if this instance is
    /// garbage collected.
    /// </summary>
    ~Metronome()
    {
        Dispose();
    }

    /// <summary>
    /// Hook payload representing a single tick of the metronome.
    /// </summary>
    /// <param name="Metronome">The metronome that ticked.</param>
    /// <param name="Delta">The delta time since the last tick</param>
    public record class OnTick(Metronome Metronome, TimeSpan Delta);
}
