using System;
using Autofac;

namespace SimLynx.Testing;

/// <summary>
/// Helpers/extensions for testing SimLynx applications.
/// </summary>
public static class SimLynxTesting
{
    extension<TApp>(SimLynx<TApp> @this)
        where TApp : SimLynxApp
    {
        /// <summary>
        /// Creates a new testing SimLynx instance with the given app type, using default configuration and
        /// registering testing-specific services.
        /// </summary>
        /// <param name="configureContainer">An optional action to configure the container before it is built.</param>
        /// <param name="parentScope">An optional parent lifetime scope for the container.</param>
        /// <returns>The testing SimLynx instance.</returns>
        public static SimLynx<TApp> CreateTesting(
            Action<ContainerBuilder>? configureContainer = null,
            ILifetimeScope? parentScope = null
        )
        {
            return new SimLynx<TApp>(
                builder =>
                {
                    builder.RegisterModule<SimLynxTestingModule>();
                    configureContainer?.Invoke(builder);
                },
                parentScope
            );
        }
    }
}
