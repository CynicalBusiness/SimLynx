using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Registration;
using Autofac.Util;

namespace SimLynx;

/// <summary>
/// Helpers for using SimLynx.
/// </summary>
public static class SimLynx
{
    /// <summary>
    /// A unique symbol that can be used to identify objects that are internal to their respective owners and
    /// are not intended to be seen/used by external consumers.
    /// </summary>
    public static readonly Symbol InternalTag = new("internal");

    /// <summary>
    /// Registers the SimLynx services with the given container builder, using a new <typeparamref name="TApp"/> instance.
    /// </summary>
    /// <typeparam name="TApp">The type of the SimLynx app.</typeparam>
    /// <param name="builder">The container builder.</param>
    /// <returns>The module registrar.</returns>
    public static IModuleRegistrar RegisterSimLynx<TApp>(this ContainerBuilder builder)
        where TApp : SimLynxApp
    {
        return builder.RegisterModule<SimLynxModule<TApp>>().IfNotRegistered(typeof(SimLynxApp));
    }
}

/// <summary>
/// Represents a self-contained SimLynx instance.
/// </summary>
/// <typeparam name="TApp">The type of the SimLynx app.</typeparam>
public class SimLynx<TApp> : Disposable
    where TApp : SimLynxApp
{
    /// <summary>
    /// Creates a new SimLynx instance with the given app type, allowing for optional container configuration
    /// before the container is built.
    /// </summary>
    /// <param name="configureContainer">An optional action to configure the container before it is built.</param>
    /// <param name="parentScope">An optional parent lifetime scope for the container.</param>
    public SimLynx(Action<ContainerBuilder>? configureContainer = null, ILifetimeScope? parentScope = null)
    {
        void configure(ContainerBuilder builder)
        {
            builder.RegisterSimLynx<TApp>();
            configureContainer?.Invoke(builder);
        }

        if (parentScope is null)
        {
            var builder = new ContainerBuilder();
            configure(builder);
            Container = builder.Build();
        }
        else
        {
            Container = parentScope.BeginLifetimeScope(configure);
        }

        App = Container.Resolve<TApp>();
    }

    /// <summary>
    /// Creates a new SimLynx instance with the given app type, using default configuration.
    /// </summary>
    public SimLynx()
        : this(null, null) { }

    /// <summary>
    /// The DI container for this SimLynx instance.
    /// </summary>
    public ILifetimeScope Container { get; private set; }

    /// <summary>
    /// The SimLynx app for this SimLynx instance.
    /// </summary>
    public TApp App { get; }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        await base.DisposeAsync(disposing);

        if (disposing)
        {
            await Container.DisposeAsync();
        }
    }
}
