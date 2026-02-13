
using System;
using Autofac;
using Microsoft.Extensions.Logging;
using SimLynx.Design;

namespace SimLynx.Core;

/// <summary>
/// Module responsible for registering prototypes in a given scope.
/// </summary>
/// <param name="logger">The logger to use for logging prototype registration.</param>
public class PrototypeRegister(ILogger<PrototypeRegister> logger) : ValueRegister<Symbol, PrototypeDef>
{
    /// <summary>
    /// Logging Event ID for when a prototype is registered.
    /// </summary>
    public static readonly EventId PrototypeRegisteredEventId = Symbol.For(nameof(LogRegisteredPrototype));

    private static readonly Action<ILogger, Symbol, Exception?> LogRegisteredPrototype
        = LoggerMessage.Define<Symbol>(
            LogLevel.Debug,
            PrototypeRegisteredEventId,
            "Registered prototype: {PrototypeName}");

    /// <inheritdoc/>
    public override bool TryRegisterValue(PrototypeDef value)
    {
        var result = base.TryRegisterValue(value);
        if (result && value.Extends is not null)
        {
            // can't get stuck on recursive since it'll bail when it loops around
            // we'll not check for cycles here
            TryRegisterValue(value.Extends);
        }
        return result;
    }

    /// <inheritdoc />
    protected override Symbol GetRegistrationKey(PrototypeDef value)
        => value.Name;

    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PrototypeResolver>()
            .AsSelf()
            .SingleInstance();

        using (logger.BeginScope("Registering prototypes..."))
        {
            foreach (var def in Values)
            {
                builder.Register(c => def.Create(c.Resolve<ILifetimeScope>()))
                    .AsSelf()
                    .Keyed<Prototype>(def.Name)
                    .InstancePerOwned<Stage>();

                LogRegisteredPrototype(logger, def.Name, null);
            }
        }

        base.Load(builder);
    }
}
