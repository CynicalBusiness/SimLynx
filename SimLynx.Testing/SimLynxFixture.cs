using System;
using Autofac;

namespace SimLynx.Testing;

/// <summary>
/// A testing fixture designed to test content and functionality which belongs to a specific phase of SimLynx.
/// <br/>
/// This fixture will, upon being constructed, create a new SimLynx instance/container.
/// </summary>
/// <remarks>
/// To use the fixture, create a subclass of it and implement the necessary members.
/// </remarks>
/// <typeparam name="TApp">The type of the SimLynx application being tested.</typeparam>
public abstract class SimLynxFixture<TApp> : IDisposable
    where TApp : SimLynxApp
{
    /// <summary>
    /// Initializes a new fixture with an optional <paramref name="parentScope"/>.
    /// </summary>
    /// <param name="parentScope">The parent lifetime scope for the container, if any.</param>
    protected SimLynxFixture(ILifetimeScope? parentScope = null)
    {
        Container = CreateContainer(parentScope);
    }

    /// <summary>
    /// The SimLynx app for this fixture.
    /// </summary>
    public TApp App => Container.Resolve<TApp>();

    /// <summary>
    /// The SimLynx container for this fixture.
    /// </summary>
    public ILifetimeScope Container { get; }

    /// <summary>
    /// Override to configure how the container is built.
    /// </summary>
    /// <returns>The created container.</returns>
    public virtual ILifetimeScope CreateContainer(ILifetimeScope? parentScope = null)
    {
        var simLynx = SimLynxTesting.CreateTesting<TApp>(
            configureContainer: ConfigureContainer,
            parentScope: parentScope
        );
        return simLynx.Container;
    }

    /// <summary>
    /// Override this method to configure the container before it is built.
    /// </summary>
    /// <param name="builder">The container builder to configure.</param>
    protected virtual void ConfigureContainer(ContainerBuilder builder)
    {
        // no-op by default
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        Container.Dispose();
    }
}
