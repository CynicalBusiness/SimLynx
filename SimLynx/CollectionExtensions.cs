using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SimLynx.Core;

namespace SimLynx;

/// <summary>
/// Provides extension methods for collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Delegate for <see cref="TrySelect{TItem, TResult}"/>
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="item"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public delegate bool TrySelectDelegate<TItem, TResult>(TItem item, [MaybeNullWhen(false)] out TResult result);

    #region IEnumerable<T>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="this">The source sequence.</param>
    extension<T>(IEnumerable<T> @this)
    {
        /// <summary>
        /// Performs the specified action on each element of the source sequence and yields the element.
        /// </summary>
        /// <param name="action">The action to perform on each element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that yields the elements of the source sequence after performing the action on each element.</returns>
        public IEnumerable<T> Tap(Action<T> action)
        {
            foreach (var item in @this)
            {
                action(item);
                yield return item;
            }
        }

        /// <summary>
        /// Enumerates the source sequence and performs the specified action on each element.
        /// </summary>
        /// <param name="action">The action to perform</param>
        public void ForEach(Action<T, int> action)
        {
            int index = 0;
            foreach (var item in @this)
            {
                action(item, index++);
            }
        }

        /// <inheritdoc cref="ForEach{T}(IEnumerable{T}, Action{T, int})"/>
        public void ForEach(Action<T> action)
        {
            @this.ForEach((item, _) => action(item));
        }

        /// <summary>
        /// Attempts to select a value from each element in the source sequence using the specified selector function.
        /// If the selector function returns <c>false</c> for an element, that element is skipped.
        /// </summary>
        /// <typeparam name="TResult">The type of the result value.</typeparam>
        /// <param name="selector">The selector function</param>
        /// <returns>An enumeration of successful selections</returns>
        public IEnumerable<TResult> TrySelect<TResult>(TrySelectDelegate<T, TResult> selector)
        {
            foreach (var item in @this)
            {
                if (selector(item, out var result))
                {
                    yield return result;
                }
            }
        }

        /// <summary>
        /// Tries to get the first element of the source sequence, returning <c>true</c> if successful and
        /// <c>false</c> if the sequence is empty.
        /// </summary>
        /// <param name="result">The first element of the sequence if successful; otherwise, the default value of <typeparamref name="T"/>.</param>
        /// <returns><c>true</c> if the first element was successfully retrieved; otherwise, <c>false</c>.</returns>
        public bool TryGetFirst([MaybeNullWhen(false)] out T result)
        {
            var enumerator = @this.GetEnumerator();
            if (enumerator.MoveNext())
            {
                result = enumerator.Current;
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Tries to get the first element of the source sequence that satisfies the specified predicate, returning
        /// <c>true</c> if successful and <c>false</c> if no such element exists.
        /// </summary>
        /// <param name="predicate">The predicate function to test each element.</param>
        /// <param name="result">The first element that satisfies the predicate if successful; otherwise, the default value of <typeparamref name="T"/>.</param>
        /// <returns><c>true</c> if an element satisfying the predicate was successfully retrieved; otherwise, <c>false</c>.</returns>
        public bool TryGetFirst(Func<T, bool> predicate, [MaybeNullWhen(false)] out T result)
        {
            return @this.Where(predicate).TryGetFirst(out result);
        }

        /// <summary>
        /// Creates a new <see cref="ReplayEnumerable{T}"/> that can be enumerated multiple times, yielding the same
        /// sequence of items each time but only ever enumerating the source once.
        /// </summary>
        /// <remarks>
        /// This is useful for expensive enumerations best performed both lazily and only once.
        /// </remarks>
        /// <returns></returns>
        public ReplayEnumerable<T> Replay()
        {
            return new ReplayEnumerable<T>(@this);
        }
    }
    #endregion

    #region IEnumerable<IEnumerable<T>>
    extension<T>(IEnumerable<IEnumerable<T>> @this)
    {
        /// <summary>
        /// Computes the Cartesian product of the sequences in the source sequence, returning an enumeration of all
        /// possible combinations of elements from each sequence.
        /// </summary>
        /// <remarks>
        /// The resulting sequences will have a length equal to the number of source sequences.
        /// <br/>
        /// Enumeration is lazy, but source sequences are enumerated multiple times based on the number of items
        /// in each sequence after it.
        /// </remarks>
        /// <returns>An enumeration of all possible combinations of elements from each sequence.</returns>
        public IEnumerable<IEnumerable<T>> CartesianProduct()
        {
            // see: https://ericlippert.com/2010/06/28/computing-a-cartesian-product-with-linq/
            IEnumerable<IEnumerable<T>> results =
            [
                [],
            ];
            return @this.Aggregate(
                results,
                (acc, seq) =>
                {
                    return acc.SelectMany(accItem => seq.Select(seqItem => accItem.Append(seqItem)));
                }
            );
        }
    }
    #endregion

    #region IReadOnlyDictionary<TypeKey, TLimit>
    extension<TLimit>(IReadOnlyTypeDictionary<TLimit> @this)
    {
        /// <summary>
        /// Attempts to retrieve a value of type <typeparamref name="T"/> associated with the specified
        /// <paramref name="key"/>. Returns <c>true</c> and sets <paramref name="value"/> if the value exists; otherwise,
        /// returns <c>false</c>.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The key associated with the value.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key if it exists; otherwise, the default value for the type.</param>
        /// <returns><c>true</c> if the value exists and is of the correct type; otherwise, <c>false</c>.</returns>
        public bool TryGetValue<T>(TypeKey<T> key, [MaybeNullWhen(false)] out T value)
            where T : TLimit
        {
            if (@this.TryGetValue((TypeKey)key, out var obj) && obj is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to retrieve a value of type <typeparamref name="T"/> associated with the specified
        /// <paramref name="id"/>. Returns <c>true</c> and sets <paramref name="value"/> if the value exists; otherwise,
        /// returns <c>false</c>.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="id">The tag associated with the value.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified tag if it
        ///     exists; otherwise, the default value for the type.
        /// </param>
        /// <returns><c>true</c> if the value exists and is of the correct type; otherwise, <c>false</c>.</returns>
        public bool TryGetValue<T>(Identifier id, [MaybeNullWhen(false)] out T value)
            where T : TLimit
        {
            return @this.TryGetValue(new TypeKey<T>(id), out value);
        }

        /// <summary>
        /// Attempts to retrieve a value of type <typeparamref name="T"/> with an empty tag. Returns <c>true</c> and
        /// sets <paramref name="value"/> if the value exists; otherwise, returns <c>false</c>.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="value">When this method returns, contains the value associated with the specified tag if it exists; otherwise, the default value for the type.</param>
        /// <returns><c>true</c> if the value exists and is of the correct type; otherwise, <c>false</c>.</returns>
        public bool TryGetValue<T>([MaybeNullWhen(false)] out T value)
            where T : TLimit
        {
            return @this.TryGetValue(Identifier.Empty, out value);
        }

        /// <summary>
        /// Determines whether the dictionary contains a value of type <typeparamref name="T"/> with an empty tag.
        /// </summary>
        /// <typeparam name="T">The type of the value to check for.</typeparam>
        /// <returns><c>true</c> if the value exists and is of the correct type; otherwise, <c>false</c>.</returns>
        public bool ContainsKey<T>()
            where T : TLimit
        {
            return @this.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Gets all pairs in the dictionary where the key's type is assignable to the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to filter the keys by.</param>
        /// <returns>An enumerable of key-value pairs where the key's type is assignable to the specified type.</returns>
        public IEnumerable<KeyValuePair<TypeKey, TLimit>> GetAll(Type type)
        {
            return @this.Where(kvp => kvp.Key.Type.IsAssignableTo(type));
        }

        /// <summary>
        /// Gets all values in the dictionary where the key's type is assignable to the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to filter the keys by.</param>
        /// <returns>An enumerable of values where the key's type is assignable to the specified type.</returns>
        public IEnumerable<TLimit> GetAllValues(Type type)
        {
            return @this.Values.Where(value => value is not null && value.GetType().IsAssignableTo(type));
        }

        /// <summary>
        /// Gets all pairs in the dictionary where the value is of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the values to retrieve.</typeparam>
        /// <returns>An enumerable of key-value pairs where the value is of type <typeparamref name="T"/>.</returns>
        public IEnumerable<KeyValuePair<TypeKey, T>> GetAll<T>()
        {
            return @this.TrySelect(
                (KeyValuePair<TypeKey, TLimit> kvp, out KeyValuePair<TypeKey, T> result) =>
                {
                    if (kvp.Value is T typedValue)
                    {
                        result = new KeyValuePair<TypeKey, T>(kvp.Key, typedValue);
                        return true;
                    }
                    result = default;
                    return false;
                }
            );
        }

        /// <summary>
        /// Gets all values in the dictionary where the value is of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the values to retrieve.</typeparam>
        /// <returns>An enumerable of values where the value is of type <typeparamref name="T"/>.</returns>
        public IEnumerable<T> GetAllValues<T>()
        {
            return @this.Values.OfType<T>();
        }
    }
    #endregion

    #region ITypeDictionary<TLimit>
    extension<TLimit>(ITypeDictionary<TLimit> @this)
    {
        /// <summary>
        /// Sets a value of type <typeparamref name="T"/> in the dictionary with an empty tag.
        /// </summary>
        /// <typeparam name="T">The type of the value to set.</typeparam>
        /// <param name="value">The value to set.</param>
        public void Set<T>(T value)
            where T : TLimit
        {
            @this.Set(Symbol.Empty, value);
        }

        /// <summary>
        /// Sets a value of type <typeparamref name="T"/> in the dictionary with the specified <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to set.</typeparam>
        /// <param name="key">The key associated with the value.</param>
        /// <param name="value">The value to set.</param>
        public void Set<T>(TypeKey<T> key, T value)
            where T : TLimit
        {
            @this.Set(key.Id, value);
        }

        /// <summary>
        /// Gets the value of type <typeparamref name="T"/> associated with the specified <paramref name="key"/>. If the
        /// value does not exist, it is created using the provided <paramref name="valueFactory"/> and added to the
        /// dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve or create.</typeparam>
        /// <param name="key">The key associated with the value.</param>
        /// <param name="valueFactory">A function that creates the value if it does not exist.</param>
        /// <returns>The existing or newly created value of type <typeparamref name="T"/>.</returns>
        public T GetOrAdd<T>(TypeKey<T> key, Func<T> valueFactory)
            where T : TLimit
        {
            return (T)@this.GetOrAdd((TypeKey)key, () => valueFactory())!;
        }

        /// <summary>
        /// Gets the value of type <typeparamref name="T"/> associated with the specified <paramref name="key"/>. If the
        /// value does not exist, it is created using its default constructor and added to the dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve or create.</typeparam>
        /// <param name="key">The key associated with the value.</param>
        /// <returns>The existing or newly created <typeparamref name="T"/>.</returns>
        public T GetOrAdd<T>(TypeKey<T> key)
            where T : TLimit, new()
        {
            static T valueFactory() => new();
            return @this.GetOrAdd(key, valueFactory);
        }

        /// <summary>
        /// Gets or adds a value of type <typeparamref name="T"/> in the dictionary with the specified
        /// <paramref name="id"/>, using the provided <paramref name="valueFactory"/> to create the value if it does
        /// not exist.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve or create.</typeparam>
        /// <param name="id">The identifier associated with the value.</param>
        /// <param name="valueFactory">A function that creates the value if it does not exist.</param>
        /// <returns>The existing or newly created <typeparamref name="T"/>.</returns>
        public T GetOrAdd<T>(Identifier id, Func<T> valueFactory)
            where T : TLimit
        {
            return @this.GetOrAdd(new TypeKey<T>(id), valueFactory);
        }

        /// <summary>
        /// Gets or adds a value of type <typeparamref name="T"/> in the dictionary with an empty identifier, using the
        /// provided <paramref name="valueFactory"/> to create the value if it does not exist.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve or create.</typeparam>
        /// <param name="valueFactory">A function that creates the value if it does not exist.</param>
        /// <returns>The existing or newly created <typeparamref name="T"/>.</returns>
        public T GetOrAdd<T>(Func<T> valueFactory)
            where T : TLimit
        {
            return @this.GetOrAdd(new TypeKey<T>(), valueFactory);
        }

        /// <summary>
        /// Gets or adds a value of type <typeparamref name="T"/> in the dictionary with the specified
        /// <paramref name="id"/>. If the value does not exist, it is created using its default constructor.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve or create.</typeparam>
        /// <param name="id">The identifier associated with the value.</param>
        /// <returns>The existing or newly created <typeparamref name="T"/>.</returns>
        public T GetOrAdd<T>(Identifier id)
            where T : TLimit, new()
        {
            return @this.GetOrAdd(new TypeKey<T>(id));
        }

        /// <summary>
        /// Gets or adds a value of type <typeparamref name="T"/> in the dictionary with an empty identifier. If the value
        /// does not exist, it is created using its default constructor.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve or create.</typeparam>
        /// <returns>The existing or newly created <typeparamref name="T"/>.</returns>
        public T GetOrAdd<T>()
            where T : TLimit, new()
        {
            return @this.GetOrAdd(new TypeKey<T>());
        }
    }
    #endregion
}
