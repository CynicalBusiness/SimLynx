using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core.Registration;
using SimLynx.Design.Prototyping.Properties;

namespace SimLynx.Design.Prototyping;

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

        /// <summary>
        /// Attempts to get a config for the given <paramref name="slotId"/> and <paramref name="configName"/>.
        /// </summary>
        /// <param name="slotId">The ID of the slot to retrieve the config from.</param>
        /// <param name="configName">The name of the config to retrieve.</param>
        /// <param name="config">The retrieved config if found; otherwise, null.</param>
        /// <returns>True if the config was found; otherwise, false.</returns>
        public bool TryGetConfig(Symbol slotId, string configName, [MaybeNullWhen(false)] out IPrototypeConfig config)
        {
            if (@this.TryGetSlot(slotId, out var slot))
            {
                config = slot[configName];
                return config is not null;
            }

            config = null;
            return false;
        }

        /// <summary>
        /// Attempts to get a config of type <typeparamref name="TConfig"/> for the given <paramref name="configName"/>.
        /// </summary>
        /// <typeparam name="TConfig">The type of the config to retrieve.</typeparam>
        /// <param name="configName">The name of the config to retrieve.</param>
        /// <param name="config">The retrieved config if found; otherwise, null.</param>
        /// <returns>True if the config was found; otherwise, false.</returns>
        public bool TryGetConfig<TConfig>(string configName, [MaybeNullWhen(false)] out TConfig config)
            where TConfig : class, IPrototypeConfig
        {
            if (@this.TryGetSlot<TConfig>(out var slot))
            {
                config = slot[configName];
                return config is not null;
            }
            config = null;
            return false;
        }

        /// <summary>
        /// Attempts to get a config slot for the given <paramref name="slotId"/>.
        /// </summary>
        /// <param name="slotId">The ID of the slot to retrieve.</param>
        /// <returns>The config slot if found; otherwise, null.</returns>
        public IPrototypeConfigSlot? GetSlot(Symbol slotId)
        {
            if (@this.TryGetSlot(slotId, out var slot))
            {
                return slot;
            }
            return null;
        }

        /// <summary>
        /// Attempts to get a config slot of type <typeparamref name="TConfig"/>.
        /// </summary>
        /// <typeparam name="TConfig">The type of the config slot to retrieve.</typeparam>
        /// <returns>The config slot if found; otherwise, null.</returns>
        public IPrototypeConfigSlotOf<TConfig>? GetSlot<TConfig>()
            where TConfig : class, IPrototypeConfig
        {
            if (@this.TryGetSlot<TConfig>(out var slot))
            {
                return slot;
            }
            return null;
        }

        /// <summary>
        /// Gets a config for the given <paramref name="slotId"/> and <paramref name="configName"/>.
        /// </summary>
        /// <param name="slotId">The ID of the slot to retrieve the config from.</param>
        /// <param name="configName">The name of the config to retrieve.</param>
        /// <returns>The config if found; otherwise, null.</returns>
        public IPrototypeConfig? GetConfig(Symbol slotId, string configName)
        {
            return @this.GetSlot(slotId)?[configName];
        }

        /// <summary>
        /// Gets a config of type <typeparamref name="TConfig"/> for the given <paramref name="configName"/>, triggering
        /// one to be created if it doesn't otherwise exist, if possible.
        /// </summary>
        /// <typeparam name="TConfig">The type of the config to retrieve.</typeparam>
        /// <param name="configName">The name of the config to retrieve.</param>
        /// <returns>The config if found; otherwise, null.</returns>
        public TConfig? GetConfig<TConfig>(string configName)
            where TConfig : class, IPrototypeConfig
        {
            return @this.GetSlot<TConfig>()?[configName];
        }

        /// <summary>
        /// Walks the prototype hierarchy, yielding all ancestor prototypes of this prototype, not including itself,
        /// starting from its immediate base prototype. If the prototype has no base, this will yield nothing.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IPrototype> GetAncestors(bool includeSelf = false)
        {
            var current = includeSelf ? @this : @this.Base;
            while (current is not null)
            {
                yield return current;
                current = current.Base;
            }
        }

        /// <summary>
        /// Walks the prototype hierarchy downward from the root prototype, yielding all ancestor prototypes of this
        /// prototype, optionally including itself.
        /// </summary>
        /// <param name="includeSelf">Whether to include the current prototype itself in the results as the last element.</param>
        /// <returns>An enumerable of ancestor prototypes starting from the root.</returns>
        public IEnumerable<IPrototype> GetAncestorsFromRoot(bool includeSelf = false)
        {
            var ancestors = @this.GetAncestors().Reverse();
            if (includeSelf)
            {
                ancestors = ancestors.Append(@this);
            }
            return ancestors;
        }
    }

    extension(IPrototypeSubject @this)
    {
        /// <summary>
        /// Checks if this object is an "instance of" the given prototype by determining if its prototype chain contains the given prototype.
        /// </summary>
        /// <param name="prototype">The prototype to check against.</param>
        /// <returns>True if the object is an instance of the given prototype; otherwise, false.</returns>
        public bool IsOf(IPrototype prototype)
        {
            return @this.Prototype.Extends(prototype);
        }
    }

    extension(ContainerBuilder @this)
    {
        /// <summary>
        /// Registers a prototype configuration slot type with the given <paramref name="def"/> in the container.
        /// </summary>
        /// <param name="def">The prototype config slot definition to register.</param>
        /// <returns>The module registrar for the registered module.</returns>
        public IModuleRegistrar RegisterPrototypeConfigSlot(PrototypeConfigSlotDef def)
        {
            return @this.RegisterModule(new PrototypeConfigModule(def));
        }
    }
}
