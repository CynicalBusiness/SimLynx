using System;

namespace SimLynx;

/// <summary>
/// Interface for a cloneable object.
/// </summary>
/// <remarks>
/// Essentially a generic form of <see cref="ICloneable"/>.
/// </remarks>
/// <typeparam name="T"></typeparam>
public interface ICloneable<out T> : ICloneable
    where T : notnull
{
    /// <inheritdoc cref="ICloneable.Clone"/>
    new T Clone();

    object ICloneable.Clone() => Clone();
}
