using System;
using System.Collections;
using System.Collections.Generic;

namespace SimLynx.Core;

/// <summary>
/// An enumerable that can be replayed multiple times, yielding the same sequence of items each time but only ever
/// enumerating the source once.
/// </summary>
/// <remarks>
/// Useful for expensive enumerations best performed both lazily and only once.
/// <br/>
/// Not thread-safe.
/// </remarks>
/// <typeparam name="T">The type of elements in the sequence.</typeparam>
/// <param name="source">The source sequence to be replayed.</param>
public class ReplayEnumerable<T>(IEnumerator<T>? source) : IEnumerable<T>, IDisposable
{
    /// <summary>
    /// Creates a new replay-able enumerable from the given <paramref name="source"/> sequence. The source sequence will
    /// only be enumerated once, and the same sequence of items will be yielded each time the returned enumerable
    /// is enumerated.
    /// </summary>
    /// <param name="source"></param>
    public ReplayEnumerable(IEnumerable<T> source)
        : this(source.GetEnumerator()) { }

    private readonly List<T> _items = [];

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        var index = 0;

        while (true)
        {
            // another user of the same instance may also be enumerating, so we recheck for each yield
            if (index < _items.Count)
            {
                yield return _items[index];
            }
            else if (source is not null && source.MoveNext())
            {
                var item = source.Current;
                _items.Add(item);
                yield return item;
            }
            else
            {
                // nothing left to enumerate
                Dispose();
                break;
            }

            index++;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public void Dispose()
    {
        source?.Dispose();
        source = null;
    }
}
