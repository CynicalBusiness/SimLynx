using Autofac;

namespace SimLynx.Design.Prototyping.Blueprints;

/// <summary>
/// An immutable compiled blueprint for a prototype, containing all information necessary to create, configure, or
/// otherwise build an object in which it is a prototype of.
/// </summary>
public interface IBlueprint
{
    /// <summary>
    /// The prototype this blueprint was compiled from.
    /// </summary>
    public IPrototype Prototype { get; }

    /// <summary>
    /// Creates a new instance of the subject object for this blueprint's <see cref="Prototype"/>.
    /// </summary>
    /// <returns>A new instance of the subject object.</returns>
    public object CreateInstance(ILifetimeScope scope);
}

/// <summary>
/// An immutable compiled blueprint for a prototype, containing all information necessary to create, configure, or
/// otherwise build a(n) <typeparamref name="TSubject"/> instance.
/// </summary>
/// <typeparam name="TSubject">The type of object this blueprint creates.</typeparam>
public interface IBlueprint<out TSubject> : IBlueprint
    where TSubject : class, IPrototypeSubject
{
    /// <summary>
    /// The prototype this blueprint was compiled from.
    /// </summary>
    public new IPrototype<TSubject> Prototype { get; }

    IPrototype IBlueprint.Prototype => Prototype;

    /// <summary>
    /// Creates a new <typeparamref name="TSubject"/> instance.
    /// </summary>
    /// <param name="scope">The Autofac scope used to resolve dependencies.</param>
    /// <returns>A new instance of <typeparamref name="TSubject"/>.</returns>
    public new TSubject CreateInstance(ILifetimeScope scope);

    object IBlueprint.CreateInstance(ILifetimeScope scope) => CreateInstance(scope);
}
