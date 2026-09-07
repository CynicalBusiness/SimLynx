using System;
using Autofac;

namespace SimLynx.Design.Prototyping.Blueprints;

/// <summary>
/// Context provided to blueprint build handlers.
/// </summary>
public interface IBlueprintBuildContext
{
    /// <summary>
    /// Per-instance context options.
    /// </summary>
    public ITypeDictionary<object> InstanceOptions { get; }

    /// <summary>
    /// The Autofac lifetime scope the subject is created in.
    /// </summary>
    public ILifetimeScope Scope { get; }

    /// <summary>
    /// The concrete type of subject being built.
    /// </summary>
    public Type SubjectType { get; }
}
