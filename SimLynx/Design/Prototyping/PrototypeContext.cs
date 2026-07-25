namespace SimLynx.Design.Prototyping;

/// <summary>
/// Context for a prototype.
/// </summary>
/// <typeparam name="TSubject">The concrete subject type of the prototype.</typeparam>
/// <param name="ConfigSlotCatalog">The catalog of configuration slots available for the prototype.</param>
public record PrototypeContext<TSubject>(PrototypeConfigSlotCatalog ConfigSlotCatalog)
    where TSubject : class, IPrototypeSubject;
