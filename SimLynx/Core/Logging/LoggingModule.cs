using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SimLynx.Core.Logging;

internal class LoggingModule(LoggingOptions options) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var sc = new ServiceCollection();

        var shouldSetupLogging = options.Setup.HasFlag(LoggingOptions.LoggerSetupFlags.Setup);
        if (shouldSetupLogging)
        {
            sc.AddLogging();
        }

        LoggingBuilder loggingBuilder = new(sc);
        if (options.Setup.HasFlag(LoggingOptions.LoggerSetupFlags.ConsoleProvider))
        {
            loggingBuilder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
            });
        }

        options.ConfigureLogging?.Invoke(loggingBuilder);

        builder.Populate(sc);

        if (!shouldSetupLogging)
        {
            // do this at the end to ensure *something* will resolve if we weren't responsible for setting it up
            // but don't step on anything upstream might configure.
            builder.RegisterInstance(NullLogger.Instance).As<ILogger>().PreserveExistingDefaults();
            builder.RegisterInstance(NullLoggerFactory.Instance).As<ILoggerFactory>().PreserveExistingDefaults();

            builder
                .RegisterGeneric(typeof(NullLogger<>))
                .As(typeof(ILogger<>))
                .SingleInstance()
                .IfNotRegistered(typeof(ILogger<LoggerRegistrationProbe>));
        }
    }

    private class LoggingBuilder(IServiceCollection services) : ILoggingBuilder
    {
        // stub to enable logging builder configuration without being the one responsible for logging.
        public IServiceCollection Services { get; } = services;
    }

    private class LoggerRegistrationProbe { }
}
