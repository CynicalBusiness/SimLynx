namespace SimLynx.Core;

/// <summary>
/// Definition for a <see cref="Stage"/>
/// </summary>
/// <param name="name">The name of the stage</param>
public class StageDef(Symbol name) : IHaveSymbolicName
{
    /// <inheritdoc/>
    public Symbol Name { get; } = name;
}
