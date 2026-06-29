namespace SimLynx.Core.Prototyping;

/// <summary>
/// Interface for objects which are intended to be created, configured, or otherwise built from a prototype.
/// </summary>
public interface IPrototypeSubject
{
    /// <summary>
    /// The prototype of this object.
    /// </summary>
    [Configurable(false)]
    public IPrototype Prototype { get; }
}

/// <summary>
/// Interface for objects which are intended to be created, configured, or otherwise built from a
/// <typeparamref name="TPrototype"/>.
/// </summary>
/// <typeparam name="TPrototype">The type of prototype.</typeparam>
public interface IPrototypeSubject<out TPrototype> : IPrototypeSubject
    where TPrototype : IPrototype
{
    /// <inheritdoc cref="IPrototypeSubject.Prototype"/>
    [Configurable(false)]
    public new TPrototype Prototype { get; }
    IPrototype IPrototypeSubject.Prototype => Prototype;
}
