
using Autofac.Builder;

namespace SimLynx.Design;

/// <summary>
/// Extensions related to the design phase.
/// </summary>
public static class DesignExtensions
{

    extension<TLimit, TActivatorData, TRegistrationStyle>(IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
    {

        /// <summary>
        /// Configures the registration to be a design instance, meaning it will be shared within the design phase but not outside of it.
        /// </summary>
        /// <returns>A registration builder to further configure the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> DesignInstance()
        {
            return builder.InstancePerOwned<DesignPhase>();
        }

    }

}
