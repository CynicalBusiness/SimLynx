using System;
using System.Threading;

namespace SimLynx.Core;

/// <summary>
/// A value similar to <see cref="System.Lazy{T}"/>, but can also be reset and will invoke the factory again when
/// next used.
/// </summary>
/// <remarks>
/// This type is thread-safe.
/// </remarks>
/// <typeparam name="T"></typeparam>
public class ResetLazy<T>(Func<T> factory)
{
    private Maybe<T> _value = Maybe<T>.None;
    private readonly ReaderWriterLockSlim _lock = new();

    /// <summary>
    /// Indicates whether this instance currently has a value cached.
    /// </summary>
    public bool HasValue
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _value.HasValue;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    /// <summary>
    /// The current value of this instance. If there is no value, or it has been reset, the factory will be invoked
    /// to create a new value.
    /// </summary>
    public T Value
    {
        get
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_value.HasValue)
                {
                    return _value.Value;
                }

                _lock.EnterWriteLock();
                try
                {
                    if (!_value.HasValue)
                    {
                        _value = new Maybe<T>(factory());
                    }

                    return _value.Value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }
    }

    /// <summary>
    /// Resets this instance, causing the next access to <see cref="Value"/> to invoke the factory again to create
    /// a new value.
    /// </summary>
    public void Reset()
    {
        _lock.EnterWriteLock();
        try
        {
            _value = Maybe<T>.None;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
