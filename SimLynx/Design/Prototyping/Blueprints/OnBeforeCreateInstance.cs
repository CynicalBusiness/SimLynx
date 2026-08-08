using System.Collections.Generic;
using Autofac.Core;

namespace SimLynx.Design.Prototyping.Blueprints;

/// <summary>
/// Hook event raised before a blueprint creates an instance of its subject type.
/// </summary>
/// <param name="Blueprint">The blueprint that is creating the instance.</param>
/// <param name="BuildContext">The context of the blueprint build process.</param>
/// <param name="Parameters">The list of parameters to be used when creating the instance.</param>
public record OnBeforeCreateInstance(
    IBlueprint Blueprint,
    IBlueprintBuildContext BuildContext,
    List<Parameter> Parameters
);
