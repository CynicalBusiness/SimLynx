namespace SimLynx.Core.Prototyping.Blueprints;

/// <summary>
/// Base implementation for a <see cref="IBlueprint{TSubject}"/>, suitable for most blueprints.
/// </summary>
/// <typeparam name="TSubject">The type of object this blueprint creates.</typeparam>
/// <param name="builder">The blueprint builder used to construct this blueprint.</param>
public abstract class Blueprint<TSubject>(BlueprintBuilder<TSubject> builder) : IBlueprint<TSubject>
    where TSubject : class, IPrototypeSubject
{
    /// <inheritdoc/>
    public IPrototype<TSubject> Prototype { get; } = builder.Prototype;

    /// <inheritdoc/>
    public TSubject CreateInstance()
    {
        throw new System.NotImplementedException();
    }
}
