namespace SimLynx.Design.Prototyping;

/// <summary>
/// Contextual information for a prototype.
/// </summary>
public interface IPrototypeContext
{
    /// <summary>
    /// The catalog of configuration slots available for the prototype.
    /// </summary>
    public PrototypeConfigSlotCatalog ConfigSlotCatalog { get; }
}
