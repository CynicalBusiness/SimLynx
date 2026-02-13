
using System;
using Autofac;

namespace SimLynx.Core;

/// <summary>
/// A type of module that is designed to be a point of registration that is subsequently used to configure a scope
/// (i.e. as a <see cref="Module"/>).
/// </summary>
public class Register : Module
{

    /// <summary>
    /// Event that is raised during the registration phase, allowing external code to register dependencies in the
    /// scope this register is producing.
    /// </summary>
    public event Action<ContainerBuilder>? OnConfigure;

    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        OnConfigure?.Invoke(builder);
    }
}
