using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace SimLynx.Design.Prototyping.Properties;

/// <summary>
/// Cached information regarding properties on a prototype subject type.
/// </summary>
public abstract class PrototypePropertyTypeInfo
{
    /// <summary>
    /// Gets the <see cref="PrototypePropertyTypeInfo"/> for the given prototype subject type.
    /// </summary>
    /// <typeparam name="TSubject">The type of the prototype subject.</typeparam>
    /// <returns>The <see cref="PrototypePropertyTypeInfo"/> for the specified prototype subject type.</returns>
    public static PrototypePropertyTypeInfo For<TSubject>()
        where TSubject : class, IPrototypeSubject
    {
        return PrototypePropertyTypeInfo<TSubject>.Instance;
    }

    /// <summary>
    /// Gets the <see cref="PrototypePropertyTypeInfo"/> for the given prototype subject type.
    /// </summary>
    /// <param name="subjectType">The type of the prototype subject.</param>
    /// <returns>The <see cref="PrototypePropertyTypeInfo"/> for the specified prototype subject type.</returns>
    /// <exception cref="ArgumentException">Thrown if the provided type is not a prototype subject class.</exception>
    public static PrototypePropertyTypeInfo For(Type subjectType)
    {
        ArgumentNullException.ThrowIfNull(subjectType, nameof(subjectType));

        if (!subjectType.IsClass || !subjectType.IsAssignableTo(typeof(IPrototypeSubject)))
        {
            throw new ArgumentException(
                $"The provided type '{subjectType.FullName}' is not a prototype subject class.",
                nameof(subjectType)
            );
        }

        var genericType = typeof(PrototypePropertyTypeInfo<>).MakeGenericType(subjectType);
        var instanceProperty = genericType.GetProperty(
            nameof(PrototypePropertyTypeInfo<>.Instance),
            BindingFlags.Public | BindingFlags.Static
        );
        return (PrototypePropertyTypeInfo)instanceProperty!.GetValue(null)!;
    }

    internal PrototypePropertyTypeInfo() { }

    /// <summary>
    /// The type of prototype subject this property type info is for.
    /// </summary>
    public abstract Type SubjectType { get; }

    /// <summary>
    /// Gets all configurable properties on the prototype subject type.
    /// </summary>
    public abstract IEnumerable<PropertyInfo> Properties { get; }

    /// <summary>
    /// Attempts to get a valid configurable property on the prototype subject type with the given
    /// <paramref name="propertyName"/>.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <param name="property">When this method returns, contains the <see cref="PropertyInfo"/> of the property if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the property was found; otherwise, <c>false</c>.</returns>
    public abstract bool TryGetProperty(string propertyName, [NotNullWhen(true)] out PropertyInfo? property);
}

internal sealed class PrototypePropertyTypeInfo<TSubject> : PrototypePropertyTypeInfo
    where TSubject : class, IPrototypeSubject
{
    public static readonly PrototypePropertyTypeInfo<TSubject> Instance = new();

    private readonly Dictionary<string, PropertyInfo> _properties;

    internal PrototypePropertyTypeInfo()
    {
        _properties = typeof(TSubject)
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(p => p.IsPrototypeConfigurable)
            .ToDictionary(p => p.Name, p => p);
    }

    public override Type SubjectType => typeof(TSubject);

    public override IEnumerable<PropertyInfo> Properties => _properties.Values;

    public override bool TryGetProperty(string propertyName, [NotNullWhen(true)] out PropertyInfo? property)
    {
        return _properties.TryGetValue(propertyName, out property);
    }
}
