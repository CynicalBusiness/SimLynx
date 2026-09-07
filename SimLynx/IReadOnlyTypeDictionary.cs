using System.Collections.Generic;

namespace SimLynx;

/// <summary>
/// A read-only dictionary that maps values by their type and optional tag.
/// </summary>
/// <typeparam name="TLimit">The upper bound type for the values stored in the dictionary.</typeparam>
public interface IReadOnlyTypeDictionary<TLimit> : IReadOnlyDictionary<TypeKey, TLimit> { }
