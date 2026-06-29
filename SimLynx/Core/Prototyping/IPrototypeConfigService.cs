namespace SimLynx.Core.Prototyping;

/// <summary>
/// Base interface for services related to prototype configs
/// </summary>
public interface IPrototypeConfigService
{
    /// <summary>
    /// The name of the slot this provider provides configs for.
    /// </summary>
    public Symbol Slot { get; }
}
