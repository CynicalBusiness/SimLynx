namespace SimLynx.Design.Prototyping.Blueprints;

/// <summary>
/// Hook event raised after a blueprint creates an instance of its subject type.
/// </summary>
/// <param name="Blueprint">The blueprint that created the instance.</param>
/// <param name="Instance">The instance that was created.</param>
public record OnCreateInstance(IBlueprint Blueprint, object Instance);
