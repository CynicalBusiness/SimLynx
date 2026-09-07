using System;

namespace SimLynx.Design.Prototyping.Parameters;

/// <summary>
/// Extension methods for prototyping injection parameters.
/// </summary>
public static class PrototypeParameterExtensions
{
    extension<TSubject>(IPrototype<TSubject> @this)
        where TSubject : class, IPrototypeSubject
    {
        /// <summary>
        /// Gets the configuration slot for injection parameters of this prototype's subject.
        /// </summary>
        public ParameterConfigSlot<TSubject> Parameters
        {
            get
            {
                if (
                    !@this.TryGetSlot<IParameterConfig<TSubject>>(out var rawSlot)
                    || rawSlot is not ParameterConfigSlot<TSubject> slot
                )
                {
                    throw new InvalidOperationException("Parameters not valid on this prototype");
                }
                return slot;
            }
        }
    }
}
