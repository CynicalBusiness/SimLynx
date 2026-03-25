using Autofac;

namespace SimLynx.Design;

/// <summary>
/// Provider of design-phase registrations, used to configure the design phase's container.
/// </summary>
public interface IDesignRegistrationProvider
{
    /// <summary>
    /// Configures the design phase container.
    /// </summary>
    /// <param name="builder">The container builder used to configure the design phase container.</param>
    public void ConfigureDesign(ContainerBuilder builder);
}
