namespace SimLynx.Core.Prototyping;

/// <summary>
/// Base implementation for a <see cref="IPrototypeBlueprint{TSubject}"/>, suitable for most blueprints.
/// </summary>
/// <typeparam name="TSubject">The type of object this blueprint creates.</typeparam>
/// <param name="prototype">The prototype this blueprint was compiled from.</param>
public abstract class PrototypeBlueprint<TSubject>(IPrototype<TSubject> prototype) : IPrototypeBlueprint<TSubject>
    where TSubject : class, IPrototypeSubject
{
    /// <inheritdoc/>
    public IPrototype<TSubject> Prototype { get; } = prototype;

    /// <inheritdoc/>
    public TSubject CreateInstance()
    {
        throw new System.NotImplementedException();
    }
}
