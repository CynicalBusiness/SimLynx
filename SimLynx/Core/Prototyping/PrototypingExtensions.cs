using System.Reflection;
using Autofac;
using Autofac.Builder;
using SimLynx.Design;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// Extension methods for prototyping-related interfaces and classes.
/// </summary>
public static class PrototypingExtensions
{
    extension(PropertyInfo @this)
    {
        /// <summary>
        /// Indicates whether this property is configurable, meaning it can be set by a prototype.
        /// </summary>
        public bool IsPrototypeConfigurable
        {
            get
            {
                var setMethod = @this.SetMethod;
                if (setMethod is null)
                {
                    return false;
                }

                var configurableAttribute = @this.GetCustomAttribute<ConfigurableAttribute>(inherit: true);
                return configurableAttribute is null ? setMethod.IsPublic : configurableAttribute.IsConfigurable;
            }
        }
    }

    extension(IPrototype @this)
    {
        /// <summary>
        /// Determines if this prototype is, or its prototype chain contains, the given prototype.
        /// </summary>
        /// <param name="prototype">The prototype to check against.</param>
        /// <returns>True if this prototype is, or its prototype chain contains, the given prototype; otherwise, false.</returns>
        public bool Extends(IPrototype prototype)
        {
            var current = @this;
            while (current is not null)
            {
                if (current == prototype)
                {
                    return true;
                }
                current = current.Base;
            }
            return false;
        }
    }

    extension(IPrototypeSubject @this)
    {
        /// <summary>
        /// Checks if this object is an "instance of" by determining if its prototype chain contains the given prototype.
        /// </summary>
        /// <param name="prototype">The prototype to check against.</param>
        /// <returns>True if the object is an instance of the given prototype; otherwise, false.</returns>
        public bool Is(IPrototype prototype)
        {
            return @this.Prototype.Extends(prototype);
        }
    }

    extension(ContainerBuilder @this)
    {
        /// <summary>
        /// Registers a prototype config resolver type with the container with the relevant service type and scope.
        /// </summary>
        /// <typeparam name="TResolver">The type of the resolver to register.</typeparam>
        /// <returns>The registration builder for further configuration.</returns>
        public IRegistrationBuilder<
            TResolver,
            ConcreteReflectionActivatorData,
            SingleRegistrationStyle
        > RegisterPrototypeConfigResolver<TResolver>(Symbol slot)
            where TResolver : class, IPrototypeConfigResolver
        {
            return @this
                .RegisterType<TResolver>()
                .As<IPrototypeConfigResolver>()
                .Keyed<IPrototypeConfigResolver>(slot)
                .WithProperty(s => s.Slot, slot)
                .DesignInstance();
        }
    }
}
