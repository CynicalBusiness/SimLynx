
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimLynx.Core.Defs;

internal class DefTypeInfo<TBaseDef, TDef>(Type defType, IDefTypeInfo<TBaseDef>? baseTypeInfo = null) : IDefTypeInfo<TDef>
    where TBaseDef : class, IDef
    where TDef : class, TBaseDef
{

    private static ImmutableDictionary<string, IDefProperty> GetProperties()
    {
        var propInfos = typeof(TDef).GetProperties(
            BindingFlags.Instance
            | BindingFlags.DeclaredOnly
            | BindingFlags.Public
            | BindingFlags.NonPublic);

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
            .ToImmutableDictionary(p => p.Name, p => (IDefProperty)new Property(p));

    }

    private static Func<TDef>? GetConstructorFunc()
    {
        if (typeof(TDef).IsAbstract)
        {
            return null;
        }

        var defaultCtor = typeof(TDef).GetConstructor(BindingFlags.Public, []);
        if (defaultCtor is null)
        {
            return null;
        }

        var newEx = Expression.New(defaultCtor);
        var lambda = Expression.Lambda<Func<TDef>>(newEx);
        return lambda.Compile();
    }

    public IReadOnlyDictionary<string, IDefProperty> OwnProperties { get; } = GetProperties();
    public Func<TDef>? CreateInstance { get; } = GetConstructorFunc();
    public IDefTypeInfo<TBaseDef>? BaseTypeInfo { get; } = baseTypeInfo;
    public Type DefType { get; } = defType;

    public IDefProperty? this[string propertyName] => TryGetProperty(propertyName, out var prop) ? prop : null;

    public IEnumerable<IDefProperty> Properties
    {
        get
        {
            var extends = BaseTypeInfo?.Properties ?? [];
            return extends
                .Where((p) => !OwnProperties.ContainsKey(p.Info.Name))
                .Concat(OwnProperties.Values);
        }
    }

    public bool TryGetProperty(string name, [NotNullWhen(true)] out IDefProperty? property)
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

    public class Property : IDefProperty
    {
        internal Property(PropertyInfo propInfo)
        {
            Info = propInfo;

            IsRequired = Info.IsDefined(typeof(System.Runtime.CompilerServices.RequiredMemberAttribute), inherit: false) // this attribute is applied by the `required` modifier
                || Info.IsDefined(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute));
        }

        public PropertyInfo Info { get; }
        public bool IsRequired { get; }
    }

}
