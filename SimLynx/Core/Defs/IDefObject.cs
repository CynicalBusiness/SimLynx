
namespace SimLynx.Core.Defs;

/// <summary>
/// Interface for objects which can be created by a relevant <see cref="IDef"/>.
/// </summary>
public interface IDefObject
{

    /// <summary>
    /// The def used to create this object.
    /// </summary>
    public IDef Def { get; }

}

/// <summary>
/// Interface for objects which can be created by a <typeparamref name="TDef"/>.
/// </summary>
/// <typeparam name="TDef">The type of definition used to create this object.</typeparam>
public interface IDefObject<out TDef> : IDefObject
    where TDef : IDef
{

    /// <summary>
    /// The definition used to create this object.
    /// </summary>
    public new TDef Def { get; }

    IDef IDefObject.Def => Def;

}
