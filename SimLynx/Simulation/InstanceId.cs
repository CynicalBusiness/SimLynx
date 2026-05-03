namespace SimLynx.Simulation;

/// <summary>
/// Identifier for a entity instance.
/// </summary>
/// <param name="Prototype">The prototype this instance is from.</param>
/// <param name="Value">The unique value for this instance within its prototype.</param>
public readonly record struct InstanceId(Symbol Prototype, int Value);
