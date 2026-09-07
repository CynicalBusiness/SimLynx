using SimLynx.Design.Prototyping;

namespace SimLynx.Simulation.ComponentModel.Prototyping;

/// <summary>
/// A prototype for an <see cref="Entity"/>.
/// </summary>
/// <typeparam name="TEntity">The type of entity.</typeparam>
/// <param name="id">The identifier of the entity prototype.</param>
/// <param name="ctx">The prototype context.</param>
public class EntityPrototype<TEntity>(Symbol id, PrototypeContext<TEntity> ctx) : ComponentPrototype<TEntity>(id, ctx)
    where TEntity : Entity { }
