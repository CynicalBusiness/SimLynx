using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimLynx.Core.Prototyping;

internal class PrototypeInfo<TBaseType, TType> : IPrototypeInfo<TType>
    where TBaseType : class, IPrototype
    where TType : class, TBaseType
{
    private static ImmutableDictionary<string, PrototypeProperty> GetProperties(IPrototypeInfo prototypeInfo)
    {
        var propInfos = typeof(TType).GetProperties(
            BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic
        );

        return propInfos
            .Where(p =>
            {
                if (p.SetMethod is null)
                {
                    return false; // Must have a setter to be configurable.
                }

                var attr = p.GetCustomAttribute<ConfigurableAttribute>();
                if (attr is null)
                {
                    return p.SetMethod.IsPublic;
                }

                return attr.IsConfigurable;
            })
            .ToImmutableDictionary(p => p.Name, p => new PrototypeProperty(prototypeInfo, p));
    }

    private static Func<TType>? GetConstructorFunc(Type type)
    {
        if (type.IsAbstract || !type.IsAssignableTo(typeof(TType)))
        {
            return null;
        }

        var defaultCtor = type.GetConstructor(BindingFlags.Public, []);
        if (defaultCtor is null)
        {
            return null;
        }

        var newEx = Expression.New(defaultCtor);
        var lambda = Expression.Lambda<Func<TType>>(newEx);
        return lambda.Compile();
    }

    public PrototypeInfo(Type type, IPrototypeInfo<TBaseType>? baseTypeInfo = null)
    {
        Type = type;
        BaseTypeInfo = baseTypeInfo;

        OwnProperties = GetProperties(this);
        CreateInstance = GetConstructorFunc(Type);
    }

    public Type Type { get; }
    public IPrototypeInfo<TBaseType>? BaseTypeInfo { get; }

    public IReadOnlyDictionary<string, PrototypeProperty> OwnProperties { get; }
    public Func<TType>? CreateInstance { get; }

    public PrototypeProperty? this[string propertyName] => TryGetProperty(propertyName, out var prop) ? prop : null;

    public IEnumerable<PrototypeProperty> Properties
    {
        get
        {
            var extends = BaseTypeInfo?.Properties ?? [];
            return extends.Where((p) => !OwnProperties.ContainsKey(p.Property.Name)).Concat(OwnProperties.Values);
        }
    }

    public bool TryGetProperty(string name, [NotNullWhen(true)] out PrototypeProperty? property)
    {
        if (OwnProperties.TryGetValue(name, out property))
        {
            return true;
        }

        if (BaseTypeInfo is not null)
        {
            return BaseTypeInfo.TryGetProperty(name, out property);
        }

        return false;
    }
}
