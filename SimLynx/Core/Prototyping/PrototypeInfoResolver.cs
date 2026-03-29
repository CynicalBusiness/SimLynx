using System;
using System.Collections.Generic;
using System.Threading;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// Resolver for type info for a given base <typeparamref name="TPrototype"/> type.
/// </summary>
public class PrototypeInfoResolver<TPrototype>
    where TPrototype : class, IPrototype
{
    private readonly Dictionary<Type, IPrototypeInfo<TPrototype>> _typeInfos = [];
    private readonly ReaderWriterLockSlim _lock = new();

    /// <summary>
    /// Checks if the given type is valid for use as a prototype type in this resolver. The type must be a class
    /// assignable to <typeparamref name="TPrototype"/>, but may be abstract or lack a default constructor and still
    /// be valid.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is valid, false otherwise.</returns>
    public static bool IsValidType(Type type)
    {
        return type.IsClass && type.IsAssignableTo(typeof(TPrototype));
    }

    /// <summary>
    /// Gets the prototype type information for the given prototype type, creating it if it does not already exist
    /// in the resolver's cache.
    /// </summary>
    /// <param name="defType">The type of prototype to get the information for.</param>
    /// <returns>The prototype type information for the given prototype type.</returns>
    public IPrototypeInfo<TPrototype> Get(Type defType)
    {
        if (defType is null)
        {
            throw new ArgumentNullException(nameof(defType));
        }

        if (!IsValidType(defType))
        {
            throw new ArgumentException(
                $"Type '{defType.FullName}' is not a class assignable to '{typeof(TPrototype).FullName}'",
                nameof(defType)
            );
        }

        _lock.EnterUpgradeableReadLock();
        try
        {
            if (_typeInfos.TryGetValue(defType, out var typeInfo))
            {
                return typeInfo;
            }

            _lock.EnterWriteLock();
            try
            {
                return CreateTypeInfo(defType);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    /// <inheritdoc cref="Get(Type)"/>
    /// <typeparam name="TType">The type of prototype to get the information for.</typeparam>
    public IPrototypeInfo<TType> Get<TType>()
        where TType : class, TPrototype
    {
        return (IPrototypeInfo<TType>)Get(typeof(TType));
    }

    private IPrototypeInfo<TPrototype> CreateTypeInfo(Type defType)
    {
        if (_typeInfos.TryGetValue(defType, out var existingTypeInfo))
        {
            return existingTypeInfo;
        }

        var baseType = defType.BaseType;
        IPrototypeInfo<TPrototype>? baseTypeInfo = null;
        if (baseType is not null && IsValidType(baseType))
        {
            baseTypeInfo = Get(baseType);
        }

        var typeInfoType = typeof(PrototypeInfo<,>).MakeGenericType(baseTypeInfo?.Type ?? typeof(TPrototype), defType);
        var typeInfo = (IPrototypeInfo<TPrototype>)Activator.CreateInstance(typeInfoType, defType, baseTypeInfo)!;

        _typeInfos[defType] = typeInfo;
        return typeInfo;
    }
}
