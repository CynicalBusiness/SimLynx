
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace SimLynx;

/// <summary>
/// Main application controller for SimLynx. Can be extended by consumer applications to provide additional
/// functionality to SimLynx.
/// </summary>
/// <remarks>
/// For most use-cases, your main application class should extend from this class.
/// </remarks>
public abstract class SimLynxApplication : Module
{

    /// <summary>
    /// Allows for configuring MS-styles services during host creation.
    /// This is only needed if specifically configuring MS-style services. For custom registrations, prefer
    /// using <see cref="Load"/>.
    /// </summary>
    /// <param name="services">The collection of services to configure</param>
    protected virtual void LoadServices(ServiceCollection services)
    {
    }

    /// <summary>
    /// Allows for configuring Autofac registrations during host creation, specifically for the design-time scope.
    /// </summary>
    /// <param name="builder">The builder for the container</param>
    protected virtual void ConfigureDesign(ContainerBuilder builder)
    {
    }

    /// <summary>
    /// Allows for configuring Autofac registrations during host creation, specifically for the runtime scope.
    /// </summary>
    /// <param name="builder">The builder for the container</param>
    protected virtual void ConfigureRuntime(ContainerBuilder builder)
    {
    }

    /// <summary>
    /// Allows for configuring the host during host creation, after the container has been built and resolved.
    /// </summary>
    /// <param name="host">The host being configured</param>
    protected virtual void Configure(SimLynxHost host)
    {
        host.Design.OnConfigure += ConfigureDesign;
        host.Runtime.OnConfigure += ConfigureRuntime;
    }

    /// <summary>
    /// Allows for loading additional registrations into the root container during host creation.
    /// </summary>
    /// <remarks>
    /// In <em>most</em> cases, you should prefer using <see cref="ConfigureDesign"/> and <see cref="ConfigureRuntime"/>
    /// for registrations, as these will be properly scoped to the design-time and runtime scopes respectively.
    /// Use this method for registrations that should be shared across both scopes, or for modifying existing
    /// registrations in the root container.
    /// </remarks>
    /// <param name="builder">The builder for the container</param>
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        builder.RegisterModule<SimLynxModule>();
    }

    internal void AddServices(ServiceCollection services)
    {
        LoadServices(services);
    }

    internal void ConfigureHost(SimLynxHost host)
    {
        Configure(host);
    }

}
