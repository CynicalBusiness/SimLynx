
using System;

namespace SimLynx.Core.Defs;

/// <summary>
/// General interface for defs.
/// </summary>
public interface IDef
{

    /// <summary>
    /// The ID of this definition.
    /// </summary>
    public Symbol Id { get; }

    /// <summary>
    /// Indicates this def is abstract, meaning it cannot be built directly and must be inherited by another def.
    /// </summary>
    public bool IsAbstract { get; }

    /// <summary>
    /// The type of object this def creates.
    /// </summary>
    public Type ObjectType { get; }

    /// <summary>
    /// Initializes this def, performing any necessary setup or validation after construction and property configuration
    /// prior to being used to build any objects.
    /// </summary>
    public void Init();

}

/// <summary>
/// General interface for defs which create <typeparamref name="TObject"/> objects.
/// </summary>
/// <typeparam name="TObject">The object type this def creates</typeparam>
public interface IDef<out TObject> : IDef
    where TObject : class, IDefObject
{
}
