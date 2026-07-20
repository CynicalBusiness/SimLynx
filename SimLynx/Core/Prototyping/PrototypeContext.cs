namespace SimLynx.Core.Prototyping;

/// <summary>
/// Context for a prototype.
/// </summary>
/// <typeparam name="TSubject">The concrete subject type of the prototype.</typeparam>
/// <param name="ConfigSlotResolverFactory">Factory for creating a <see cref="PrototypeConfigSlotResolver{TSubject}"/> instance.</param>
public record PrototypeContext<TSubject>(PrototypeConfigSlotResolver<TSubject>.Factory ConfigSlotResolverFactory)
    where TSubject : class, IPrototypeSubject;
