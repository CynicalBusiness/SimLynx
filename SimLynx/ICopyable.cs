namespace SimLynx;

/// <summary>
/// Interface which marks objects which can be copied into a <typeparamref name="TTarget"/> object.
/// </summary>
/// <remarks>
/// Conceptually similar to <see cref="System.ICloneable"/>, but is meant for copying into an existing object rather than
/// creating a new one. It is also contravariant, rather than the covariance of <see cref="ICloneable{T}"/>.
/// </remarks>
/// <typeparam name="TTarget">The type of object to copy to.</typeparam>
public interface ICopyable<in TTarget>
{
    /// <summary>
    /// Copies this object into the <paramref name="target"/> object.
    /// </summary>
    /// <param name="target">The target to copy into.</param>
    public void CopyTo(TTarget target);
}
