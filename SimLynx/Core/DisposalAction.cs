using System;

namespace SimLynx.Core;

/// <summary>
/// Helper class to invoke an action when disposed.
/// </summary>
public class DisposalAction(Action action) : IDisposable
{
    /// <summary>
    /// Event invoked when this instance is disposed, which will invoke the provided action.
    /// </summary>
    public event Action OnDispose = action;

    /// <summary>
    /// Disposes this instance, invoking the provided action.
    /// </summary>
    public void Dispose()
    {
        OnDispose?.Invoke();
    }
}
