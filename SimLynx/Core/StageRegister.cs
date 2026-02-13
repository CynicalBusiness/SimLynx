
using System;
using Autofac;
using Microsoft.Extensions.Logging;

namespace SimLynx.Core;

/// <summary>
/// Module for registering stage-specific dependencies.
/// </summary>
public class StageRegister(ILogger<StageRegister> logger) : ValueRegister<Symbol, StageDef>
{
    /// <summary>
    /// Logging Event ID for when a stage is registered.
    /// </summary>
    public static readonly EventId StageRegisteredEventId = Symbol.For(nameof(LogRegisteredStage));

    private static readonly Action<ILogger, Symbol, Exception?> LogRegisteredStage = LoggerMessage.Define<Symbol>(
        LogLevel.Information,
        StageRegisteredEventId,
        "Registered stage: {StageName}");

    /// <inheritdoc/>
    protected override Symbol GetRegistrationKey(StageDef value)
        => value.Name;

    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<StageResolver>()
            .AsSelf()
            .SingleInstance();

        using (logger.BeginScope("Registering stages..."))
        {
            foreach (var def in Values)
            {
                builder.RegisterType<Stage>()
                    .AsSelf()
                    .WithParameter("name", def.Name)
                    .Keyed<Stage>(def.Name)
                    .SingleInstance();
            }
        }

        base.Load(builder);
    }

}
