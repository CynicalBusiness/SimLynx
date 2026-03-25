namespace SimLynx.Core;

/// <summary>
/// Interface for objects that have a symbolic name.
/// </summary>
public interface IHaveSymbolicName
{
    /// <summary>
    /// The symbolic name of this object.
    /// </summary>
    public Symbol Name { get; }
}
