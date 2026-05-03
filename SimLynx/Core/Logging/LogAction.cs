using System;
using Microsoft.Extensions.Logging;

namespace SimLynx.Core.Logging;

#pragma warning disable CS1712 // false positive

/// <summary>
/// A logging action that can be invoked with a logger and an optional exception to log something.
/// </summary>
/// <param name="logger">The logger to use for logging.</param>
/// <param name="exception">An optional exception to include in the log.</param>
public delegate void LogAction(ILogger? logger, Exception? exception = null);

/// <inheritdoc cref="LogAction"/>
/// <typeparam name="T1">The type of the first argument to include in the log.</typeparam>
public delegate void LogAction<T1>(ILogger? logger, T1 arg1, Exception? exception = null);

/// <inheritdoc cref="LogAction{T1}"/>
/// <typeparam name="T2">The type of the second argument to include in the log.</typeparam>
public delegate void LogAction<T1, T2>(ILogger? logger, T1 arg1, T2 arg2, Exception? exception = null);

/// <inheritdoc cref="LogAction{T1, T2}"/>
/// <typeparam name="T3">The type of the third argument to include in the log.</typeparam>
public delegate void LogAction<T1, T2, T3>(ILogger? logger, T1 arg1, T2 arg2, T3 arg3, Exception? exception = null);

/// <inheritdoc cref="LogAction{T1, T2, T3}"/>
/// <typeparam name="T4">The type of the fourth argument to include in the log.</typeparam>
public delegate void LogAction<T1, T2, T3, T4>(
    ILogger? logger,
    T1 arg1,
    T2 arg2,
    T3 arg3,
    T4 arg4,
    Exception? exception = null
);

/// <inheritdoc cref="LogAction{T1, T2, T3, T4}"/>
/// <typeparam name="T5">The type of the fifth argument to include in the log.</typeparam>
public delegate void LogAction<T1, T2, T3, T4, T5>(
    ILogger? logger,
    T1 arg1,
    T2 arg2,
    T3 arg3,
    T4 arg4,
    T5 arg5,
    Exception? exception = null
);

#pragma warning restore CS1712
