
using Autofac.Features.Indexed;

namespace SimLynx.Core;

/// <summary>
/// Resolver for stages, allowing retrieval of stages by name.
/// </summary>
/// <param name="stageIndex">The index of stages keyed by their symbols.</param>
public class StageResolver(IIndex<Symbol, Stage> stageIndex)
{

    /// <summary>
    /// Gets the stage with the given name.
    /// </summary>
    /// <param name="name">The name of the stage to retrieve.</param>
    /// <returns>The stage with the given name.</returns>
    public Stage this[Symbol name] => stageIndex[name];

    /// <summary>
    /// Tries to get the stage with the given name.
    /// </summary>
    /// <param name="name">The name of the stage to retrieve.</param>
    /// <param name="stage">The stage with the given name, if found; otherwise, null.</param>
    /// <returns>True if the stage was found; otherwise, false.</returns>
    public bool TryGetStage(Symbol name, out Stage? stage)
    {
        return stageIndex.TryGetValue(name, out stage);
    }

}
