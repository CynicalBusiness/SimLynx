using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;

namespace SimLynx.Core;

/// <summary>
/// Autofac middleware that enables injection of non-required properties using the <see cref="NamedPropertyParameter"/>.
/// </summary>
/// <param name="selector">The property selector used to determine which properties to inject.</param>
public class ParameterizedPropertyMiddleware(IPropertySelector selector) : IResolveMiddleware
{
    /// <inheritdoc/>
    public PipelinePhase Phase => PipelinePhase.Activation;

    /// <inheritdoc/>
    public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
    {
        // let Autofac make the instance first
        next(context);

        var instance = context.Instance!;
        var instanceType = instance.GetType();

        foreach (var param in context.Parameters.OfType<NamedPropertyParameter>())
        {
            var prop = TypeCache.GetProperty(instanceType, param.Name);
            if (!selector.InjectProperty(prop.Info, instance))
            {
                continue;
            }

            prop.ValidateValue(param.Value);
            prop.Set(instance, param.Value);
        }
    }

    private class TypeCache(Type type)
    {
        public delegate void PropertySetter(object instance, object? value);

        public static readonly Dictionary<Type, TypeCache> Cache = [];

        public static TypeCache Get(Type type)
        {
            if (!Cache.TryGetValue(type, out var typeCache))
            {
                typeCache = new TypeCache(type);
                Cache[type] = typeCache;
            }

            return typeCache;
        }

        public static Property GetProperty(Type type, string propertyName)
        {
            return Get(type).Get(propertyName);
        }

        public Type Type { get; } = type;
        public Dictionary<string, Property> Properties { get; } = [];

        public Property Get(string propertyName)
        {
            if (!Properties.TryGetValue(propertyName, out var property))
            {
                var propertyInfo = Type.GetProperty(propertyName);
                if (propertyInfo is null || propertyInfo.SetMethod is null)
                {
                    throw new ArgumentException(
                        $"Property {propertyName} does not exist on type {Type} or has no set/init.",
                        nameof(propertyName)
                    );
                }

                var setter = propertyInfo.CreatePropertySetter();
                property = new Property(propertyInfo, setter);
                Properties[propertyName] = property;
            }

            return property;
        }

        public record Property(PropertyInfo Info, Action<object, object?> Set)
        {
            public void ValidateValue(object? value)
            {
                if (value is null)
                {
                    if (Info.PropertyType.IsValueType && Nullable.GetUnderlyingType(Info.PropertyType) is null)
                    {
                        throw new DependencyResolutionException(
                            $"Cannot assign null to non-nullable property {Info.Name} of type {Info.PropertyType}."
                        );
                    }
                }
                else if (!Info.PropertyType.IsInstanceOfType(value))
                {
                    throw new DependencyResolutionException(
                        $"Cannot assign value of type {value.GetType()} to property {Info.Name} of type {Info.PropertyType}."
                    );
                }
            }
        }
    }
}
