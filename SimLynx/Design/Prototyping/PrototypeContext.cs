namespace SimLynx.Design.Prototyping;

/// <summary>
/// Context for a prototype.
/// </summary>
/// <param name="ConfigSlotCatalog">The catalog of configuration slots available for the prototype.</param>
public record PrototypeContext(PrototypeConfigSlotCatalog ConfigSlotCatalog) : IPrototypeContext;
